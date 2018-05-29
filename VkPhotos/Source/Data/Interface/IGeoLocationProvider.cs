using System.Threading.Tasks;

namespace VkPhotos.Data
{
    public interface IGeoLocationProvider
    {
        Task<IGeoLocation> GetLocationAsync();
    }
}
