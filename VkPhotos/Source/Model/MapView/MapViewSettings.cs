using System;
using VkPhotos.Model.Map;

namespace VkPhotos.Model
{
    public class MapViewSettings
    {
        public GeoPoint Location { get; }
        public Double ZoomLevel { get; }
        public DateTime FromDate { get; }
        public DateTime ToDate { get; }

        public MapViewSettings(DateTime fromDate, DateTime toDate, Double latitude, Double longitude, Double zoomLevel)
        {
            Location = new GeoPoint(latitude, longitude);
            ZoomLevel = zoomLevel;
            FromDate = fromDate;
            ToDate = toDate;
        }
    }
}
