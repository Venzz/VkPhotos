using System;
using System.Threading.Tasks;
using VkPhotos.Data;
using VkPhotos.Data.IpApi;
using VkPhotos.Data.Vk;

namespace VkPhotos.Model
{
    public class ApplicationModel
    {
        private IStatsProvider Stats;

        public PhotoCollection Photos { get; private set; }
        public MapView MapView { get; }
        public LocalFileCollection LocalFiles { get; }
        public Users Users { get; }
        public Boolean VkConnectAuthorizationFailed { get; set; }



        public ApplicationModel()
        {
            #if DEBUG
            Stats = new Data.Debug.DebugStatsProvider();
            #else
            Stats = new VkStatsProvider();
            #endif
            MapView = new MapView(new IpApiGeoLocationProvider());
            Photos = new PhotoCollection(new VkPhotoProvider());
            LocalFiles = new LocalFileCollection();
            Users = new Users(new VkUserProvider());
        }

        public Task InitializeAsync()
        {
            if (App.Settings.AccessToken != null)
                return Stats.TrackVisitorAsync(App.Settings.AccessToken);
            return Task.CompletedTask;
        }
    }
}
