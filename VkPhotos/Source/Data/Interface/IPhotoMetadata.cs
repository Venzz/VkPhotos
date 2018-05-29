using System;
using System.Collections.Generic;

namespace VkPhotos.Data
{
    public interface IPhotoMetadata
    {
        UInt32? Id { get; }
        OwnerId Owner { get; }
        Double? Latitude { get; }
        Double? Longitude { get; }
        DateTime? Date { get; }
        String Text { get; }
        IReadOnlyCollection<IPhoto> Photos { get; }
    }
}
