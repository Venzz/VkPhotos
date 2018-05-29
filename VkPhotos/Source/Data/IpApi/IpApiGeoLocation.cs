using System;
using Windows.Data.Json;

namespace VkPhotos.Data.IpApi
{
    public class IpApiGeoLocation: IGeoLocation
    {
        public Double Latitude { get; private set; }
        public Double Longitude { get; private set; }

        private IpApiGeoLocation() { }

        public static IGeoLocation Create(JsonObject value)
        {
            if (!value.ContainsKey("lat") || !value.ContainsKey("lon"))
                return null;

            var instance = new IpApiGeoLocation();
            instance.Latitude = value["lat"].GetNumber();
            instance.Longitude = value["lon"].GetNumber();
            return instance;
        }
    }
}
