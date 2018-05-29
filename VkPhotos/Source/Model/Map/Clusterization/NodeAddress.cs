using System;
using System.Collections.Generic;
using Windows.Foundation;

namespace VkPhotos.Model.Map.Clusterization
{
    public class NodeAddress
    {
        public LinkedList<NodeAddressCell> Path { get; private set; }

        private NodeAddress()
        {
            Path = new LinkedList<NodeAddressCell>();
        }

        public static NodeAddress Create(Byte level, Rect bounds) => Create(level, new GeoPoint(bounds.Y, bounds.X));

        public static NodeAddress Create(Byte level, GeoPoint point)
        {
            var tilesAmount = 1 << level;
            var x = (Int32)(((180 + point.Longitude) / 360) * tilesAmount);
            var y = (Int32)(((90 - point.Latitude) / 180) * tilesAmount);

            var addressCell = new NodeAddressCell(x, y, level);
            return Create(addressCell);
        }

        public static NodeAddress Create(NodeAddressCell cell)
        {
            var address = new NodeAddress();
            var addressCell = cell;
            while (addressCell != null)
            {
                address.Path.AddFirst(addressCell);
                addressCell = addressCell.GetAncestor();
            }
            return address;
        }

        public NodeAddress Modify(Int32 x, Int32 y)
        {
            var newLastNode = Path.Last.Value.Modify(x, y);
            if (newLastNode == null)
                return null;

            var newPath = new LinkedList<NodeAddressCell>(Path);
            newPath.RemoveLast();
            newPath.AddLast(newLastNode);
            return new NodeAddress() { Path = newPath };
        }

        public override String ToString() => String.Join(" -> ", Path);
    }
}
