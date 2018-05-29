using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VkPhotos.Data;
using VkPhotos.Model;
using VkPhotos.Model.Map;

namespace VkPhotos.Screenshots
{
    public class ScreenshotsPhotoCollection: PhotoCollection
    {
        private IPhotoProvider Provider;

        public ScreenshotsPhotoCollection(): base(new ScreenshotsPhotoProvider())
        {
            Provider = new ScreenshotsPhotoProvider();
        }

        public override async Task<PhotoBatchResult> GetUsersAsync(User user)
        {
            var getResult = await Provider.GetAllAsync(user.Id, user.AccessToken, 0, 200).ConfigureAwait(false);
            var photos = new List<Photo>();
            foreach (var photoMetadata in getResult.Photos)
            {
                var photo = ScreenshotsPhoto.Create(photoMetadata);
                if (photo != null)
                    photos.Add(photo);
            }
            return new PhotoBatchResult(photos, PhotoTag.Personal, (offset, count) => Provider.GetAllAsync(user.Id, user.AccessToken, offset, count), 200);
        }
    }
}
