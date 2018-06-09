using System;
using System.Threading.Tasks;

namespace VkPhotos.Data.Debug
{
    public class DebugStatsProvider: IStatsProvider
    {
        public Task TrackVisitorAsync(String accessToken) => Task.FromResult<Object>(null);
    }
}
