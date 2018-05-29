using System;
using System.Threading.Tasks;

namespace VkPhotos.Data.Vk
{
    public class VkStatsProvider: IStatsProvider
    {
        private IHttpClient Client = new WindowsStoreHttpClient();

        public async Task TrackVisitorAsync(String accessToken)
        {
            try
            {
                var requestUrl = $"https://api.vk.com/method/stats.trackVisitor?access_token={accessToken}&v=5.42";
                await Client.GetAsync(requestUrl).ConfigureAwait(false);
            }
            catch (Exception)
            {
            }
        }
    }
}
