using System;
using System.Threading.Tasks;

namespace VkPhotos.Data
{
    public interface IPhotoProvider
    {
        Task<PhotosSearchResult> SearchAsync(Double latitude, Double longitude, DateTime startTime, DateTime endTime, UInt32 offset, UInt32 count, UInt32 radius);
        Task<PhotosSearchResult> GetAllAsync(UInt32 owner, String accessToken, UInt32 offset, UInt32 count);
    }
}
