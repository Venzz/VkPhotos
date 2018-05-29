using System.Threading.Tasks;
using Windows.Data.Json;

namespace VkPhotos.Data.IpApi
{
    public class IpApiGeoLocationProvider: IGeoLocationProvider
    {
        private IHttpClient Client = new WindowsStoreHttpClient();

        public async Task<IGeoLocation> GetLocationAsync()
        {
            var response = await Client.GetAsync($"http://ip-api.com/json/").ConfigureAwait(false);
            var geoLocation = IpApiGeoLocation.Create(JsonObject.Parse(response));
            return geoLocation;
        }
    }
}
