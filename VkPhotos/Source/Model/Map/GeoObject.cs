using System;

namespace VkPhotos.Model.Map
{
    public class GeoObject
    {
        public GeoPoint Location { get; }
        public Object Value { get; }

        public GeoObject(GeoPoint location, Object value)
        {
            Location = location;
            Value = value;
        }
    }
}
