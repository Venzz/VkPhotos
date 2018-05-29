using System;

namespace VkPhotos.Model.Map
{
    public class GeoTile
    {
        public GeoPoint Point { get; }
        public Double Width { get; }
        public Double Height { get; }



        public GeoTile(GeoPoint upperLeftPoint, Byte zoomLevel): this(upperLeftPoint, 360.0 / (1 << zoomLevel), 180.0 / (1 << zoomLevel))
        {
        }

        public GeoTile(GeoPoint upperLeftPoint, Double width, Double height)
        {
            Width = width;
            Height = height;
            Point = upperLeftPoint;
        }

        public static GeoTile Create(GeoPoint geoPoint, Byte zoomLevel)
        {
            var width = 360.0 / (1 << zoomLevel);
            var height = 180.0 / (1 << zoomLevel);

            var x = (Int32)((geoPoint.Longitude + 180.0) / 360.0 * (1 << zoomLevel));
            var y = (Int32)((90 - geoPoint.Latitude) / 180.0 * (1 << zoomLevel));
            return new GeoTile(new GeoPoint(90.0 - y * height, -180.0 + x * width), width, height);
        }

        public Boolean Contains(Double? longitude, Double? latitude)
        {
            var containsLongitude = (!longitude.HasValue || (longitude.Value >= Point.Longitude) && (longitude.Value < Point.Longitude + Width));
            var containsLatitude = (!latitude.HasValue || (latitude.Value <= Point.Latitude) && (latitude.Value > Point.Latitude - Height));
            return containsLongitude && containsLatitude;
        }

        public GeoPoint GetCenter() => new GeoPoint(Point.Latitude - Height / 2, Point.Longitude + Width / 2);

        public Boolean IsInvalid() => (Point.Longitude < -180) || (Point.Latitude > 90) || (Point.Longitude + Width > 180) || (Point.Latitude - Height < -90);

        public override String ToString() => $"{Point.Latitude}, {Point.Longitude}";

        public override Int32 GetHashCode() => (Int32)((((Point.Longitude * 251) + Point.Latitude) * 251 + Width) * 251 + Height);

        public override Boolean Equals(Object obj)
        {
            if (obj == null)
                return false;
            if (!(obj is GeoTile))
                return false;

            var geoTile = (GeoTile)obj;
            return (Point.Longitude == geoTile.Point.Longitude) && (Point.Latitude == geoTile.Point.Latitude) && (Width == geoTile.Width) && (Height == geoTile.Height);
        }
    }
}
