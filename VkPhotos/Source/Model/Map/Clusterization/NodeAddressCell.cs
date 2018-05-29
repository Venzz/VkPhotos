using System;

namespace VkPhotos.Model.Map.Clusterization
{
    public class NodeAddressCell
    {
        public Int32 X { get; }
        public Int32 Y { get; }
        public Byte Level { get; }

        public NodeAddressCell(Int32 x, Int32 y, Byte level)
        {
            X = x;
            Y = y;
            Level = level;
        }

        public NodeAddressCell GetAncestor()
        {
            if (Level <= 1)
                return null;
            var x = (Int32)((Double)X / (1 << Level) * (1 << (Level - 1)));
            var y = (Int32)((Double)Y / (1 << Level) * (1 << (Level - 1)));
            return new NodeAddressCell(x, y, (Byte)(Level - 1));
        }

        public NodeAddressCell Modify(Int32 x, Int32 y)
        {
            var newX = X - x;
            var newY = Y - y;
            var tiles = (1 << Level);
            if ((newX < 0) || (newX >= tiles) || (newY < 0) || (newY >= tiles))
                return null;
            return new NodeAddressCell(newX, newY, Level);
        }

        public override String ToString() => $"{Level}:{X},{Y}";
    }
}
