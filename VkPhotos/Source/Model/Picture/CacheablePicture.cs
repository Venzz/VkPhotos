using System;
using System.Threading.Tasks;
using Venz.Images;
using VkPhotos.Data;
using Windows.Foundation;
using Windows.Storage.Streams;

namespace VkPhotos.Model
{
    public class CacheablePicture: StreamPicture
    {
        private static IHttpClient Client = new WindowsStoreHttpClient();

        private UInt32 Id;
        private IPhoto Photo;

        public override Boolean UseAsyncPattern => true;
        public override Size? Size => Photo.Size;



        public CacheablePicture(UInt32 id, IPhoto photo)
        {
            Id = id;
            Photo = photo;
        }

        public override async Task<IRandomAccessStream> GetStreamAsync()
        {
            var inMemory = new InMemoryRandomAccessStream();
            var storageFile = App.Model.LocalFiles.TryGet(Id);
            if (storageFile != null)
            {
                using (var stream = await storageFile.OpenReadAsync().AsTask().ConfigureAwait(false))
                    await RandomAccessStream.CopyAsync(stream, inMemory).AsTask().ConfigureAwait(false);
            }
            else
            {
                var content = await Client.DownloadAsync(Photo.Source).ConfigureAwait(false);
                if (App.Settings.IsCachingEnabled)
                    await App.Model.LocalFiles.StoreAsync(Id, content).ConfigureAwait(false);
                await inMemory.WriteAsync(content).AsTask().ConfigureAwait(false);
            }
            return inMemory;
        }
    }
}
