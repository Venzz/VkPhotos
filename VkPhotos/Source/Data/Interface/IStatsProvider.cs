using System;
using System.Threading.Tasks;

namespace VkPhotos.Data
{
    public interface IStatsProvider
    {
        Task TrackVisitorAsync(String accessToken);
    }
}
