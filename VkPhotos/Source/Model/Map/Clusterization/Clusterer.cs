using System;

namespace VkPhotos.Model.Map.Clusterization
{
    public class Clusterer
    {
        private Node Root;

        public Clusterer()
        {
            Root = new Node(new GeoTile(new GeoPoint(90, -180), 0));
        }

        public void AddObject(GeoObject geoObject) => Root.AddObject(geoObject);

        public Node GetNode(Byte zoomLevel, GeoPoint center)
        {
            var address = NodeAddress.Create(zoomLevel, center);
            var clustererNode = Root;
            var node = address.Path.First;
            while (node != address.Path.Last)
            {
                var addressCell = node.Value;
                var x = addressCell.X % 2;
                var y = addressCell.Y % 2;

                clustererNode = clustererNode.GetChildNode(x, y);
                node = node.Next;
            }

            clustererNode = clustererNode.GetChildNode(node.Value.X % 2, node.Value.Y % 2);
            clustererNode.GatherObjects();
            return clustererNode;
        }
    }
}
