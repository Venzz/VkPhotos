using System;
using System.Collections.Generic;
using VkPhotos.Data;

namespace VkPhotos.Screenshots
{
    public class ScreenshotsPhotoMetadata: IPhotoMetadata
    {
        public UInt32? Id { get; }
        public OwnerId Owner { get; }
        public Double? Latitude { get; }
        public Double? Longitude { get; }
        public DateTime? Date { get; }
        public String Text { get; }
        public IReadOnlyCollection<IPhoto> Photos { get; }



        private ScreenshotsPhotoMetadata() { }

        public ScreenshotsPhotoMetadata(UInt32? id, OwnerId owner, Double? latitude, Double? longitude, DateTime? date, String text, IReadOnlyCollection<IPhoto> photos)
        {
            Id = id;
            Owner = owner;
            Latitude = latitude;
            Longitude = longitude;
            Date = date;
            Text = text;
            Photos = photos;
        }
    }
}
