using System;
using System.Collections.Generic;
using Venz.Extensions;
using Venz.Images;
using VkPhotos.Data;
using VkPhotos.Model.Map;
using Windows.Foundation;

namespace VkPhotos.Model
{
    public class Photo: IMapObjectElementContext
    {
        private const Int32 PreviewBoundary = 100;
        private const Int32 LargePreviewBoundary = 400;

        public UInt32 Id { get; protected set; }
        public OwnerId Owner { get; protected set; }
        public DateTime Date { get; protected set; }
        public String Text { get; protected set; }
        public GeoPoint Location { get; protected set; }
        public Picture Preview { get; protected set; }
        public Picture LargePreview { get; protected set; }
        public Picture Original { get; protected set; }
        public Size Size { get; protected set; }
        public PhotoTag Tag { get; set; }



        protected Photo() { }

        public static Photo Create(IPhotoMetadata photoMetadata)
        {
            if (!photoMetadata.Latitude.HasValue || !photoMetadata.Longitude.HasValue || (photoMetadata.Photos.Count == 0) || !photoMetadata.Id.HasValue || !photoMetadata.Date.HasValue || (photoMetadata.Photos.Count == 0))
                return null;

            var photoSizes = new PhotoSizesCollection(photoMetadata.Photos);
            var instance = new Photo();
            instance.Id = photoMetadata.Id.Value;
            instance.Owner = photoMetadata.Owner;
            instance.Location = new GeoPoint(photoMetadata.Latitude.Value, photoMetadata.Longitude.Value);
            instance.Preview = new CacheablePicture(photoMetadata.Id.Value, photoSizes.Preview);
            instance.LargePreview = new InMemoryPicture(photoSizes.LargePreview);
            instance.Original = new InMemoryPicture(photoSizes.Original);
            instance.Size = photoSizes.Size;
            instance.Date = photoMetadata.Date.Value;
            instance.Text = photoMetadata.Text;
            return instance;
        }

        private class PhotoSizesCollection
        {
            private IReadOnlyCollection<IPhoto> Source;

            public Size Size { get; }
            public IPhoto Original { get; }
            public IPhoto LargePreview { get; }
            public IPhoto Preview { get; }

            public PhotoSizesCollection(IReadOnlyCollection<IPhoto> source)
            {
                Source = source;
                var smallestLargePreviewDifference = (Double)Int32.MaxValue;
                var smallestPreviewDifference = (Double)Int32.MaxValue;
                foreach (var photo in source)
                {
                    if ((Original == null) || (photo.Size.Width > Original.Size.Width) && (photo.Size.Height > Original.Size.Height)
                            || (photo.Size.Width > Original.Size.Width) && (photo.Size.Height == Original.Size.Height)
                            || (photo.Size.Width == Original.Size.Width) && (photo.Size.Height > Original.Size.Height))
                    {
                        Original = photo;
                    }

                    if (LargePreview == null)
                    {
                        LargePreview = photo;
                    }
                    else if ((photo.Size.IsPortrait() && (Math.Abs(photo.Size.Width - LargePreviewBoundary) < smallestLargePreviewDifference))
                         || (photo.Size.IsLandscape() && (Math.Abs(photo.Size.Height - LargePreviewBoundary) < smallestLargePreviewDifference)))
                    {
                        smallestLargePreviewDifference = photo.Size.IsPortrait() ? Math.Abs(photo.Size.Width - LargePreviewBoundary) : Math.Abs(photo.Size.Height - LargePreviewBoundary);
                        LargePreview = photo;
                    }

                    if ((Preview == null) || (photo.Size.IsPortrait() && (Math.Abs(photo.Size.Width - PreviewBoundary) < smallestPreviewDifference))
                            || (photo.Size.IsLandscape() && (Math.Abs(photo.Size.Height - PreviewBoundary) < smallestPreviewDifference)))
                    {
                        smallestPreviewDifference = photo.Size.IsPortrait() ? Math.Abs(photo.Size.Width - PreviewBoundary) : Math.Abs(photo.Size.Height - PreviewBoundary);
                        Preview = photo;
                    }
                }
                Size = Original.Size;
            }
        }
    }
}
