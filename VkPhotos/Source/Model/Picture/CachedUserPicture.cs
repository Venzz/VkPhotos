using System;
using System.Threading.Tasks;
using Venz.Images;
using Windows.Storage;
using Windows.Storage.Streams;

namespace VkPhotos.Model
{
    public class CachedUserPicture: StreamPicture
    {
        public override Boolean UseAsyncPattern => true;

        public CachedUserPicture() { }

        public override async Task<IRandomAccessStream> GetStreamAsync()
        {
            var inMemory = new InMemoryRandomAccessStream();
            var storageFile = await ApplicationData.Current.LocalFolder.CreateFileAsync("User.jpg", CreationCollisionOption.OpenIfExists).AsTask().ConfigureAwait(false);
            using (var stream = await storageFile.OpenReadAsync().AsTask().ConfigureAwait(false))
                await RandomAccessStream.CopyAsync(stream, inMemory).AsTask().ConfigureAwait(false);
            return inMemory;
        }
    }
}
