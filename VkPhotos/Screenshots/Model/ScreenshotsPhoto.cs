using System.Linq;
using VkPhotos.Data;
using VkPhotos.Model.Map;

namespace VkPhotos.Screenshots
{
    public class ScreenshotsPhoto: Model.Photo
    {
        private ScreenshotsPhoto() { }

        public new static ScreenshotsPhoto Create(IPhotoMetadata photoMetadata)
        {
            var instance = new ScreenshotsPhoto();
            instance.Id = photoMetadata.Id.Value;
            instance.Owner = photoMetadata.Owner;
            instance.Location = new GeoPoint(photoMetadata.Latitude.Value, photoMetadata.Longitude.Value);
            instance.Preview = new ScreenshotsPreviewPicture(photoMetadata.Photos.First());
            instance.LargePreview = new ScreenshotsPicture(photoMetadata.Photos.First());
            instance.Original = new ScreenshotsPicture(photoMetadata.Photos.First());
            instance.Size = photoMetadata.Photos.First().Size;
            instance.Date = photoMetadata.Date.Value;
            instance.Text = photoMetadata.Text;
            return instance;
        }
    }
}
