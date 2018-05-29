using System;
using System.Threading.Tasks;
using Venz.Images;
using VkPhotos.Data;
using Windows.Foundation;
using Windows.Storage.Streams;

namespace VkPhotos.Model
{
    public class InMemoryPicture: StreamPicture, IPictureStreamReadinessInformation
    {
        private static IHttpClient Client = new WindowsStoreHttpClient();
        private IPhoto Photo;

        public Boolean IsStreamReady => false;
        public override Boolean UseAsyncPattern => true;
        public override Size? Size => Photo.Size;

        public event TypedEventHandler<IPictureStreamReadinessInformation, IRandomAccessStream> Ready = delegate { };



        public InMemoryPicture(IPhoto photo)
        {
            Photo = photo;
        }

        public override async Task<IRandomAccessStream> GetStreamAsync()
        {
            var content = await Client.DownloadAsync(Photo.Source).ConfigureAwait(false);
            var stream = new InMemoryRandomAccessStream();
            await stream.WriteAsync(content).AsTask().ConfigureAwait(false);
            Ready(this, stream);
            return stream;
        }
    }
}
