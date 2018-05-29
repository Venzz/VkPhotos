using System;
using System.Collections.Generic;
using Venz.Core;
using VkPhotos.Model.Map.Clusterization;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls.Maps;

namespace VkPhotos.Model.Map
{
    public class MapObjects<TObject, TCluster> where TObject: UIElement, IMapObjectElement, new() where TCluster: UIElement, IMapClusterElement, new()
    {
        private static TimeSpan EventFrequency = TimeSpan.FromMilliseconds(100);
        private const Int32 MaximumUnclusteredElements = 2;

        private UInt16 HorizontalTiles;
        private UInt16 VerticalTiles;
        private MapControl Map;
        private TileMatrix<Object> Matrix;
        private MapControlEventSampler EventSampler;
        private Clusterer Clusterer;
        private MapObjectsFilter Filter;
        private ObjectPool<TObject, TCluster> ObjectPool;

        public event TypedEventHandler<TObject, Object> ObjectTapped = delegate { };
        public event TypedEventHandler<TCluster, Object> ClusterTapped = delegate { };



        public MapObjects(MapControl map, UInt16 horizontalTiles, UInt16 verticalTiles)
        {
            HorizontalTiles = horizontalTiles;
            VerticalTiles = verticalTiles;
            ObjectPool = new ObjectPool<TObject, TCluster>(HorizontalTiles, VerticalTiles, MaximumUnclusteredElements);
            ObjectPool.ObjectTapped += (sender, context) => ObjectTapped(sender, context);
            ObjectPool.ClusterTapped += (sender, context) => ClusterTapped(sender, context);

            Map = map;
            Filter = new MapObjectsFilter();
            Clusterer = new Clusterer();
            Matrix = new TileMatrix<Object>(HorizontalTiles, VerticalTiles, map.ZoomLevel, new GeoPoint(map.Center.Position.Latitude, map.Center.Position.Longitude));

            EventSampler = new MapControlEventSampler(map, EventFrequency);
            EventSampler.MapViewChanged += OnMapViewChanged;
        }

        public void TriggerCurrentViewRerendering() => EventSampler.TriggerViewChange(new MapViewChangeCommands() { Recreate = true });

        public void Apply(UInt16 horizontalTiles, UInt16 verticalTiles)
        {
            if ((HorizontalTiles == horizontalTiles) && (VerticalTiles == verticalTiles))
                return;

            HorizontalTiles = horizontalTiles;
            VerticalTiles = verticalTiles;
            EventSampler.TriggerViewChange(new MapViewChangeCommands() { HorizontalTiles = horizontalTiles, VerticalTiles = verticalTiles });
        }

        public void Add(GeoObject geoObject)
        {
            lock (this)
                Clusterer.AddObject(geoObject);
        }

        private async void OnMapViewChanged(MapControlEventSampler.MapViewChangedEventArgs args)
        {
            var command = (MapViewChangeCommands)args.Parameter;
            var recreate = (command != null) && command.Recreate;
            var horizontalTiles = (command?.HorizontalTiles).HasValue ? command.HorizontalTiles.Value : HorizontalTiles;
            var verticalTiles = (command?.VerticalTiles).HasValue ? command.VerticalTiles.Value : VerticalTiles;
            var deferral = args.GetDeferral();

            try
            {
                var newTileWithNodesData = new List<TileWithNodesData>();
                lock (this)
                {
                    var changes = Matrix.Apply(horizontalTiles, verticalTiles, args.ZoomLevel, args.Center, recreate);
                    foreach (var removedTile in changes.Removed)
                    {
                        foreach (var element in removedTile.Objects)
                        {
                            if (element is TCluster)
                                ObjectPool.StoreCluster((TCluster)element);
                            else
                                ObjectPool.StoreObject((TObject)element);
                        }
                    }
 
                    foreach (var tileWithObjects in changes.Added)
                    {
                        if (tileWithObjects.Tile.IsInvalid())
                            continue;

                        var node = Clusterer.GetNode(args.ZoomLevel, tileWithObjects.Tile.GetCenter());
                        var nodeData = new NodeData(node.GetCenter(), Filter.Apply(node.GeoObjects));
                        newTileWithNodesData.Add(new TileWithNodesData(tileWithObjects, new List<NodeData>() { nodeData }));
                    }
                }
                
                await App.Dispatcher.RunAsync(() =>
                {
                    if ((command?.HorizontalTiles).HasValue && (command?.VerticalTiles).HasValue)
                        ObjectPool.Apply(Map, horizontalTiles, verticalTiles, MaximumUnclusteredElements);

                    foreach (var newTileWithNodeData in newTileWithNodesData)
                    {
                        foreach (var nodeData in newTileWithNodeData.NodesData)
                        {
                            if (nodeData.Objects.Count <= MaximumUnclusteredElements)
                            {
                                foreach (var geoObject in nodeData.Objects)
                                {
                                    if (!(geoObject.Value is IMapObjectElementContext))
                                        continue;

                                    var objectElement = ObjectPool.UseObject(Map, ((IMapObjectElementContext)geoObject.Value).Id);
                                    newTileWithNodeData.Tile.Objects.Add(objectElement);

                                    objectElement.SetContext(geoObject.Value);
                                    MapControl.SetNormalizedAnchorPoint(objectElement, objectElement.NormalizedAnchorPoint);
                                    MapControl.SetLocation(objectElement, geoObject.Location.ToGeopoint());
                                    objectElement.Show();
                                }
                            }
                            else
                            {
                                var clusterElement = ObjectPool.UseCluster(Map);
                                newTileWithNodeData.Tile.Objects.Add(clusterElement);

                                clusterElement.SetContext(nodeData.Objects);
                                MapControl.SetNormalizedAnchorPoint(clusterElement, new Point(0.5, 0.5));
                                MapControl.SetLocation(clusterElement, nodeData.Center.ToGeopoint());
                                clusterElement.Show();
                            }
                        }
                    }
                    ObjectPool.Commit(Map);
                });
            }
            finally
            {
                deferral.Complete();
            }
        }

        public void SetFilter(IMapObjectsFilter filter)
        {
            Filter.Set(filter);
            TriggerCurrentViewRerendering();
        }

        private class MapViewChangeCommands
        {
            public Boolean Recreate { get; set; }
            public UInt16? HorizontalTiles { get; set; }
            public UInt16? VerticalTiles { get; set; }
        }

        private class TileWithNodesData
        {
            public GeoTileWithObjects<Object> Tile { get; }
            public IEnumerable<NodeData> NodesData { get; }
            public TileWithNodesData(GeoTileWithObjects<Object> tile, IEnumerable<NodeData> nodesData) { Tile = tile; NodesData = nodesData; }
        }

        private class NodeData
        {
            public GeoPoint Center { get; }
            public IReadOnlyCollection<GeoObject> Objects { get; }
            public NodeData(GeoPoint center, IReadOnlyCollection<GeoObject> objects) { Center = center; Objects = objects; }
        }
    }
}
