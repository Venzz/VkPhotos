using System;
using System.Threading.Tasks;
using Venz.Images;
using VkPhotos.Data;
using Windows.Storage;
using Windows.Storage.Streams;

namespace VkPhotos.Model
{
    public class UserPicture: StreamPicture
    {
        private static IHttpClient Client = new WindowsStoreHttpClient();

        private UInt32 Id;
        private Uri Photo;
        private IRandomAccessStream PhotoStream;

        public override Boolean UseAsyncPattern => PhotoStream == null;



        public UserPicture(UInt32 id, Uri photo)
        {
            Id = id;
            Photo = photo;
        }

        public override IRandomAccessStream GetStream() => PhotoStream;

        public override async Task<IRandomAccessStream> GetStreamAsync()
        {
            if (PhotoStream != null)
                return PhotoStream;

            var inMemory = new InMemoryRandomAccessStream();
            var storageFile = await ApplicationData.Current.LocalFolder.CreateFileAsync("User.jpg", CreationCollisionOption.ReplaceExisting).AsTask().ConfigureAwait(false);
            var content = await Client.DownloadAsync(Photo).ConfigureAwait(false);
            using (var stream = await storageFile.OpenAsync(FileAccessMode.ReadWrite).AsTask().ConfigureAwait(false))
                await stream.WriteAsync(content).AsTask().ConfigureAwait(false);
            await inMemory.WriteAsync(content).AsTask().ConfigureAwait(false);
            PhotoStream = inMemory;
            return PhotoStream;
        }
    }
}
