using System;
using System.Collections.Generic;

namespace VkPhotos.Model.Map
{
    public class GeoTileWithObjects<T>
    {
        public GeoTile Tile { get; }
        public IList<T> Objects { get; }

        public GeoTileWithObjects(GeoTile geoTile)
        {
            Tile = geoTile;
            Objects = new List<T>();
        }

        public GeoTileWithObjects(GeoPoint point, Byte zoomLevel)
        {
            Objects = new List<T>();
            Tile = GeoTile.Create(point, zoomLevel);
        }

        public override Boolean Equals(Object obj)
        {
            if (obj == null)
                return false;
            if (!(obj is GeoTileWithObjects<T>))
                return false;

            var geoTileWithObjects = (GeoTileWithObjects<T>)obj;
            return Tile.Equals(geoTileWithObjects.Tile);
        }

        public override Int32 GetHashCode() => Tile.GetHashCode();

        public override String ToString() => $"Objects: {Objects.Count}, Geo: {Tile.Point}";
    }
}
