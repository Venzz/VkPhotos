using System;
using System.Threading.Tasks;
using Venz.Async;

namespace VkPhotos.Model.Map
{
    public partial class MapControlEventSampler
    {
        public delegate void MapViewChangedEventHandler(MapViewChangedEventArgs args);
        public event MapViewChangedEventHandler MapViewChanged = delegate { };



        private Task OnMapViewChangedAsync(Byte zoomLevel, GeoPoint center, Object manuallyTriggeredChangeParameter)
        {
            var mapViewChangedEventArgs = new MapViewChangedEventArgs(zoomLevel, center, manuallyTriggeredChangeParameter);
            MapViewChanged(mapViewChangedEventArgs);
            return mapViewChangedEventArgs.WaitCompletionAsync();
        }

        public class MapViewChangedEventArgs: DeferralSupportedEventArgs
        {
            public Byte ZoomLevel { get; }
            public GeoPoint Center { get; }
            public Object Parameter { get; }
            public MapViewChangedEventArgs(Byte zoomLevel, GeoPoint center, Object parameter) { ZoomLevel = zoomLevel; Center = center; Parameter = parameter; }
        }
    }
}
