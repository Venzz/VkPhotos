using System;
using System.Threading.Tasks;
using VkPhotos.Data;
using VkPhotos.Model.Map;
using Windows.Devices.Geolocation;

namespace VkPhotos.Model
{
    public class MapView
    {
        private IGeoLocationProvider GeoLocationProvider;

        public Boolean Available => (App.Settings.MapLatitude != 0) || (App.Settings.MapLongitude != 0) || (App.Settings.MapZoomLevel != 0);
        public MapViewSettings SharedLinkSettings { get; private set; }



        public MapView(IGeoLocationProvider geoLocationProvider)
        {
            GeoLocationProvider = geoLocationProvider;
        }

        public MapViewSettings Get()
        {
            var dates = GetDates();
            if (Available)
                return new MapViewSettings(dates.Item1, dates.Item2, App.Settings.MapLatitude, App.Settings.MapLongitude, App.Settings.MapZoomLevel);
            return new MapViewSettings(dates.Item1, dates.Item2, 0, 0, 6);
        }

        public async Task<MapViewSettings> GetAsync()
        {
            try
            {
                var dates = GetDates();
                var geoLocation = await GeoLocationProvider.GetLocationAsync().ConfigureAwait(false);
                return (geoLocation != null) ? new MapViewSettings(dates.Item1, dates.Item2, geoLocation.Latitude, geoLocation.Longitude, 12) : null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<MapViewSettings> GetCurrentAsync()
        {
            var dates = GetDates();
            var geolocator = new Geolocator() { DesiredAccuracyInMeters = 500 };
            var geoPosition = await geolocator.GetGeopositionAsync();
            return new MapViewSettings(dates.Item1, dates.Item2, geoPosition.Coordinate.Point.Position.Latitude, geoPosition.Coordinate.Point.Position.Longitude, 12);
        }

        public void Store(GeoPoint geoPoint, Double zoomLevel)
        {
            App.Settings.MapLatitude = geoPoint.Latitude;
            App.Settings.MapLongitude = geoPoint.Longitude;
            App.Settings.MapZoomLevel = zoomLevel;
        }

        public void SetSharedLinkSettings(MapViewSettings settings) => SharedLinkSettings = settings;



        private static Tuple<DateTime, DateTime> GetDates()
        {
            if (App.Settings.IsPastPeriodEnabled)
                return App.Settings.PastPeriod.GetDates();
            else if (App.Settings.IsPeriodEnabled)
                return new Tuple<DateTime, DateTime>(App.Settings.FromDate, App.Settings.ToDate);
            else
                throw new InvalidOperationException();
        }
    }
}
