using System;
using System.Collections.Generic;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;

namespace VkPhotos.Model.Map
{
    public class ObjectPool<TObject, TCluster> where TObject: UIElement, IMapObjectElement, new() where TCluster: UIElement, IMapClusterElement, new()
    {
        private Queue<TCluster> AvailableClusters = new Queue<TCluster>();
        private LinkedList<TObject> AvailableObjects = new LinkedList<TObject>();
        private List<TCluster> UsedClusters = new List<TCluster>();
        private IDictionary<UInt32, LinkedListNode<TObject>> UsedObjectsDictionary = new Dictionary<UInt32, LinkedListNode<TObject>>();

        private Queue<TCluster> PendingClusters = new Queue<TCluster>();
        private IDictionary<UInt32, TObject> PendingObjectsDictionary = new Dictionary<UInt32, TObject>();

        public event TypedEventHandler<TObject, Object> ObjectTapped = delegate { };
        public event TypedEventHandler<TCluster, Object> ClusterTapped = delegate { };



        public ObjectPool(UInt16 horizontalTiles, UInt16 verticalTiles, UInt16 maximumUnclusteredElements)
        {
            var totalHorizontalTiles = horizontalTiles * 2 + 1;
            var totalVerticalTiles = verticalTiles * 2 + 1;
            var subTilesInTile = 1;
            var maximumVisibleObjects = totalHorizontalTiles * totalVerticalTiles * subTilesInTile * maximumUnclusteredElements;
            var maximumVisibleClusters = totalHorizontalTiles * totalVerticalTiles * subTilesInTile;

            for (var i = 0; i < maximumVisibleObjects; i++)
            {
                var element = new TObject() { Opacity = 0 };
                element.Tapped += OnObjectTapped;
                AvailableObjects.AddLast(element);
            }
            for (var i = 0; i < maximumVisibleClusters; i++)
            {
                var element = new TCluster() { Opacity = 0 };
                element.Tapped += OnClusterTapped;
                AvailableClusters.Enqueue(element);
            }
        }

        private void OnObjectTapped(Object sender, TappedRoutedEventArgs args) => ObjectTapped((TObject)sender, ((TObject)sender).GetContext());

        private void OnClusterTapped(Object sender, TappedRoutedEventArgs args) => ClusterTapped((TCluster)sender, null);

        public void Apply(Windows.UI.Xaml.Controls.Maps.MapControl map, UInt16 horizontalTiles, UInt16 verticalTiles, UInt16 maximumUnclusteredElements)
        {
            var totalHorizontalTiles = horizontalTiles * 2 + 1;
            var totalVerticalTiles = verticalTiles * 2 + 1;
            var subTilesInTile = 4;
            var maximumVisibleObjects = totalHorizontalTiles * totalVerticalTiles * subTilesInTile * maximumUnclusteredElements;
            var maximumVisibleClusters = totalHorizontalTiles * totalVerticalTiles * subTilesInTile;

            if (maximumVisibleObjects > AvailableObjects.Count)
            {
                for (var i = 0; i < maximumVisibleObjects - AvailableObjects.Count; i++)
                {
                    var element = new TObject() { Opacity = 0 };
                    element.Tapped += OnObjectTapped;
                    AvailableObjects.AddLast(element);
                }
            }
            else if (maximumVisibleObjects < AvailableObjects.Count)
            {
                for (var i = 0; i < AvailableObjects.Count - maximumVisibleObjects; i++)
                {
                    if (UsedObjectsDictionary.Remove(AvailableObjects.First.Value.Id))
                        map.Children.Remove(AvailableObjects.First.Value);
                    PendingObjectsDictionary.Remove(AvailableObjects.First.Value.Id);
                    AvailableObjects.First.Value.Tapped -= OnObjectTapped;
                    AvailableObjects.RemoveFirst();
                }
            }

            if (maximumVisibleClusters > AvailableClusters.Count)
            {
                for (var i = 0; i < maximumVisibleClusters - AvailableClusters.Count; i++)
                {
                    var element = new TCluster() { Opacity = 0 };
                    element.Tapped += OnClusterTapped;
                    AvailableClusters.Enqueue(element);
                }
            }
            else if (maximumVisibleClusters < AvailableClusters.Count)
            {
                for (var i = 0; i < AvailableClusters.Count - maximumVisibleClusters; i++)
                {
                    var element = AvailableClusters.Dequeue();
                    map.Children.Remove(element);
                    element.Tapped -= OnClusterTapped;
                }
            }
        }

        public TCluster UseCluster(Windows.UI.Xaml.Controls.Maps.MapControl map)
        {
            if (PendingClusters.Count > 0)
                return PendingClusters.Dequeue();

            var TCluster = AvailableClusters.Dequeue();
            if (!UsedClusters.Contains(TCluster))
            {
                map.Children.Add(TCluster);
                UsedClusters.Add(TCluster);
            }
            return TCluster;
        }

        public TObject UseObject(Windows.UI.Xaml.Controls.Maps.MapControl map, UInt32 id)
        {
            if (PendingObjectsDictionary.ContainsKey(id))
            {
                var objectElement = PendingObjectsDictionary[id];
                PendingObjectsDictionary.Remove(id);
                return objectElement;
            }
            else if (UsedObjectsDictionary.ContainsKey(id))
            {
                var objectElementNode = UsedObjectsDictionary[id];
                UsedObjectsDictionary.Remove(id);
                AvailableObjects.Remove(objectElementNode);
                return objectElementNode.Value;
            }
            else
            {
                var objectElementNode = AvailableObjects.First;
                AvailableObjects.RemoveFirst();
                UsedObjectsDictionary.Remove(objectElementNode.Value.Id);
                if (!map.Children.Contains(objectElementNode.Value))
                    map.Children.Add(objectElementNode.Value);
                return objectElementNode.Value;
            }
        }

        public void StoreCluster(TCluster clusterElement) => PendingClusters.Enqueue(clusterElement);

        public void StoreObject(TObject objectElement) => PendingObjectsDictionary.Add(objectElement.Id, objectElement);

        public void Commit(Windows.UI.Xaml.Controls.Maps.MapControl map)
        {
            while (PendingClusters.Count > 0)
            {
                var clusterElement = PendingClusters.Dequeue();
                clusterElement.Hide();
                AvailableClusters.Enqueue(clusterElement);
            }
            foreach (var value in PendingObjectsDictionary.Values)
            {
                value.Hide();
                var objectElementNode = AvailableObjects.AddLast(value);
                UsedObjectsDictionary.Add(value.Id, objectElementNode);
            }
            PendingObjectsDictionary.Clear();
        }
    }
}
