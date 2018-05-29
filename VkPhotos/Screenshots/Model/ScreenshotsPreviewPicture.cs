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
    public class ScreenshotsPreviewPicture: StreamPicture, IPictureStreamReadinessInformation
    {
        private IPhoto Photo;

        public Boolean IsStreamReady => false;
        public override Boolean UseAsyncPattern => true;

        public event TypedEventHandler<IPictureStreamReadinessInformation, IRandomAccessStream> Ready = delegate { };



        public ScreenshotsPreviewPicture(IPhoto photo) { Photo = photo; }

        public override async Task<IRandomAccessStream> GetStreamAsync()
        {
            var file = await StorageFile.GetFileFromApplicationUriAsync(Photo.Source);
            var thumb = await file.GetThumbnailAsync(Windows.Storage.FileProperties.ThumbnailMode.SingleItem);
            var stream = new InMemoryRandomAccessStream();
            await RandomAccessStream.CopyAsync(thumb, stream);
            Ready(this, stream);
            return stream;
        }
    }
}
