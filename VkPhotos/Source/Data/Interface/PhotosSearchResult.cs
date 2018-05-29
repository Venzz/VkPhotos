using System;
using System.Collections.Generic;

namespace VkPhotos.Data
{
    public class PhotosSearchResult
    {
        public UInt32 Total { get; }
        public DateTime? StartDate { get; }
        public DateTime? EndDate { get; }
        public IReadOnlyCollection<IPhotoMetadata> Photos { get; }

        public PhotosSearchResult(UInt32 total, IReadOnlyCollection<IPhotoMetadata> photos)
        {
            Total = total;
            Photos = photos;
        }

        public PhotosSearchResult(UInt32 total, DateTime? startDate, DateTime? endDate, IReadOnlyCollection<IPhotoMetadata> photos)
        {
            Total = total;
            StartDate = startDate;
            EndDate = endDate;
            Photos = photos;
        }
    }
}
