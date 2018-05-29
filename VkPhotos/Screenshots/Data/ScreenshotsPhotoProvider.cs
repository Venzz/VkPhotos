using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VkPhotos.Data;
using VkPhotos.Model.Map;
using Windows.Foundation;
using Windows.Storage;

namespace VkPhotos.Screenshots
{
    public class ScreenshotsPhotoProvider: IPhotoProvider
    {
        private List<GeoPoint> Points = new List<GeoPoint>()
        {
            new GeoPoint(56.3022565254551, 38.1786244341437), // Ferma
            new GeoPoint(56.3047752506834, 38.1929168185354), // Lesnaya
            new GeoPoint(56.2881672601666, 38.1774474142526), // Pticegrad
            new GeoPoint(56.2942328123863, 38.1663497981367), // Ostrovok
            new GeoPoint(56.2966587637807, 38.1861909905864), // Glinkovo
            new GeoPoint(56.3044021166825, 38.1584469502966), // Park (3 times)
            new GeoPoint(56.2909668654075, 38.1517211223476), // Skobanka (5 times)
        };

        public async Task<PhotosSearchResult> GetAllAsync(UInt32 owner, String accessToken, UInt32 offset, UInt32 count)
        {
            var photos = new List<IPhotoMetadata>();
            photos.Add(await GetPhotoMetadataAsync(1, 0, 0, "ms-appx:///Screenshots/lisplogo_alien_256.jpg"));
            var result = new PhotosSearchResult((UInt32)photos.Count, DateTime.Now, DateTime.Now, photos);
            return await Task.FromResult<PhotosSearchResult>(result);
        }

        public async Task<PhotosSearchResult> SearchAsync(Double latitude, Double longitude, DateTime startTime, DateTime endTime, UInt32 offset, UInt32 count, UInt32 radius)
        {
            var photos = new List<IPhotoMetadata>();
            photos.Add(await GetPhotoMetadataAsync(0, Points[0].Latitude, Points[0].Longitude, "ms-appx:///Screenshots/0.jpg"));
            photos.Add(await GetPhotoMetadataAsync(1, Points[1].Latitude, Points[1].Longitude, "ms-appx:///Screenshots/1.jpg"));
            photos.Add(await GetPhotoMetadataAsync(2, Points[2].Latitude, Points[2].Longitude, "ms-appx:///Screenshots/2.jpg"));
            photos.Add(await GetPhotoMetadataAsync(3, Points[3].Latitude, Points[3].Longitude, "ms-appx:///Screenshots/3.jpg"));
            photos.Add(await GetPhotoMetadataAsync(4, Points[4].Latitude, Points[4].Longitude, "ms-appx:///Screenshots/4.jpg"));

            photos.Add(await GetPhotoMetadataAsync(5, Points[5].Latitude, Points[5].Longitude, "ms-appx:///Screenshots/5.jpg"));
            photos.Add(await GetPhotoMetadataAsync(6, Points[5].Latitude, Points[5].Longitude, "ms-appx:///Screenshots/6.jpg"));
            photos.Add(await GetPhotoMetadataAsync(7, Points[5].Latitude, Points[5].Longitude, "ms-appx:///Screenshots/7.jpg"));

            photos.Add(await GetPhotoMetadataAsync(8, Points[6].Latitude, Points[6].Longitude, "ms-appx:///Screenshots/8.jpg"));
            photos.Add(await GetPhotoMetadataAsync(9, Points[6].Latitude, Points[6].Longitude, "ms-appx:///Screenshots/8.jpg"));
            photos.Add(await GetPhotoMetadataAsync(10, Points[6].Latitude, Points[6].Longitude, "ms-appx:///Screenshots/8.jpg"));
            photos.Add(await GetPhotoMetadataAsync(11, Points[6].Latitude, Points[6].Longitude, "ms-appx:///Screenshots/8.jpg"));
            photos.Add(await GetPhotoMetadataAsync(12, Points[6].Latitude, Points[6].Longitude, "ms-appx:///Screenshots/8.jpg"));
            var result = new PhotosSearchResult((UInt32)photos.Count, DateTime.Now, DateTime.Now, photos);
            return await Task.FromResult<PhotosSearchResult>(result);
        }

        private async Task<IPhotoMetadata> GetPhotoMetadataAsync(UInt32 photoId, Double latitude, Double longitude, String uri)
        {
            var storageFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri(uri));
            var fileBuffer = await FileIO.ReadBufferAsync(storageFile);
            var imageProperties = await storageFile.Properties.GetImagePropertiesAsync();

            await App.Model.LocalFiles.StoreAsync(photoId, fileBuffer);

            var photo = new ScreenshotsDataPhoto(new Size(imageProperties.Width, imageProperties.Height), new Uri(uri), "m");
            return new ScreenshotsPhotoMetadata(photoId, OwnerId.Create(1, OwnerId.OwnerType.User), latitude, longitude, DateTime.Now, null, new IPhoto[] { photo });
        }
    }
}
