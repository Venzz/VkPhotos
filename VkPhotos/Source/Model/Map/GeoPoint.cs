using System;
using System.Globalization;
using Windows.Devices.Geolocation;

namespace VkPhotos.Model.Map
{
    public class GeoPoint
    {
        public Double Latitude { get; }
        public Double Longitude { get; }

        public GeoPoint(Double latitude, Double longitude)
        {
            Latitude = latitude;
            Longitude = longitude;
        }

        public Geopoint ToGeopoint() => new Geopoint(new BasicGeoposition() { Latitude = Latitude, Longitude = Longitude });

        public override String ToString() => $"Lat: {Latitude.ToString(CultureInfo.InvariantCulture)}, Long: {Longitude.ToString(CultureInfo.InvariantCulture)}";
    }
}
