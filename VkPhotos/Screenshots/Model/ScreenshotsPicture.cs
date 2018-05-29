using System;
using System.Threading.Tasks;
using Venz.Images;
using VkPhotos.Data;
using VkPhotos.Model;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.Streams;

namespace VkPhotos.Screenshots
{
    public class ScreenshotsPicture: StreamPicture, IPictureStreamReadinessInformation
    {
        private IPhoto Photo;

        public Boolean IsStreamReady => false;
        public override Boolean UseAsyncPattern => true;

        public event TypedEventHandler<IPictureStreamReadinessInformation, IRandomAccessStream> Ready = delegate { };



        public ScreenshotsPicture(IPhoto photo) { Photo = photo; }

        public override async Task<IRandomAccessStream> GetStreamAsync()
        {
            var file = await StorageFile.GetFileFromApplicationUriAsync(Photo.Source);
            var content = await FileIO.ReadBufferAsync(file);
            var stream = new InMemoryRandomAccessStream();
            await stream.WriteAsync(content).AsTask().ConfigureAwait(false);
            Ready(this, stream);
            return stream;
        }
    }
}
