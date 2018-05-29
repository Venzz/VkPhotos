using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VkPhotos.Data;
using VkPhotos.Model.Map;

namespace VkPhotos.Model
{
    public class PhotoCollection
    {
        private IPhotoProvider Provider;

        public PhotoCollection(IPhotoProvider provider)
        {
            Provider = provider;
        }

        public ProgressivePhotoSearch CreateProgressiveSearch(PhotoTag tag, GeoPoint location, DateTime startDate, DateTime endDate) => new ProgressivePhotoSearch(Provider, tag, location, startDate, endDate, 100);

        public virtual async Task<PhotoBatchResult> GetUsersAsync(User user)
        {
            var getResult = await Provider.GetAllAsync(user.Id, user.AccessToken, 0, 200).ConfigureAwait(false);
            var photos = new List<Photo>();
            foreach (var photoMetadata in getResult.Photos)
            {
                var photo = Photo.Create(photoMetadata);
                if (photo != null)
                {
                    photos.Add(photo);
                    photo.Tag = PhotoTag.Personal;
                }
            }
            return new PhotoBatchResult(photos, PhotoTag.Personal, (offset, count) => Provider.GetAllAsync(user.Id, user.AccessToken, offset, count), 200);
        }
    }
}
