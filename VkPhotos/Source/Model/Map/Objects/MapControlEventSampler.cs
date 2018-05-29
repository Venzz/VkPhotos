using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls.Maps;

namespace VkPhotos.Model.Map
{
    public partial class MapControlEventSampler
    {
        private Object Sync = new Object();
        private TimeSpan Frequency;
        private DateTime RecentViewChangedDate;
        private Object ManuallyTriggeredChangeParameter;
        private Boolean IsLocked;

        public Byte ZoomLevel { get; private set; }
        public GeoPoint Center { get; private set; }



        public MapControlEventSampler(MapControl map, TimeSpan frequency)
        {
            Frequency = frequency;
            Center = new GeoPoint(map.Center.Position.Latitude, map.Center.Position.Longitude);
            ZoomLevel = (Byte)map.ZoomLevel;

            map.CenterChanged += (sender, args) =>
            {
                lock (Sync)
                {
                    Center = new GeoPoint(map.Center.Position.Latitude, map.Center.Position.Longitude);
                    RecentViewChangedDate = DateTime.Now;
                    Monitor.Pulse(Sync);
                }
            };
            map.ZoomLevelChanged += (sender, args) =>
            {
                if (ZoomLevel != (Byte)sender.ZoomLevel)
                {
                    lock (Sync)
                    {
                        ZoomLevel = (Byte)map.ZoomLevel;
                        RecentViewChangedDate = DateTime.Now;
                        Monitor.Pulse(Sync);
                    }
                }
            };
            Task.Run(async () => await StartSamplerAsync());
        }

        public void TriggerViewChange(Object parameter)
        {
            lock (Sync)
            {
                IsLocked = false;
                ManuallyTriggeredChangeParameter = parameter;
                Monitor.Pulse(Sync);
            }
        }

        public void Lock() => IsLocked = true;

        private async Task StartSamplerAsync()
        {
            var processedViewChangedEvent = RecentViewChangedDate;
            var manuallyTriggeredChangeParameter = (Object)null;
            var zoomLevel = (Byte)0;
            var center = (GeoPoint)null;

            while (true)
            {
                lock (Sync)
                {
                    if (IsLocked || (ManuallyTriggeredChangeParameter == null) && (processedViewChangedEvent == RecentViewChangedDate))
                        Monitor.Wait(Sync);

                    manuallyTriggeredChangeParameter = ManuallyTriggeredChangeParameter;
                    zoomLevel = ZoomLevel;
                    center = Center;
                    processedViewChangedEvent = RecentViewChangedDate;
                    ManuallyTriggeredChangeParameter = null;
                }

                var startDate = DateTime.Now;
                await OnMapViewChangedAsync(zoomLevel, center, manuallyTriggeredChangeParameter).ConfigureAwait(false);
                var diff = Frequency.TotalMilliseconds - (DateTime.Now - startDate).TotalMilliseconds;
                if (diff > 0)
                    await Task.Delay((Int32)diff).ConfigureAwait(false);
            }
        }
    }
}
