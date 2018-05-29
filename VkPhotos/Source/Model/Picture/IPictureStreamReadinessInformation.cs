using Windows.Foundation;
using Windows.Storage.Streams;

namespace VkPhotos.Model
{
    public interface IPictureStreamReadinessInformation
    {
        event TypedEventHandler<IPictureStreamReadinessInformation, IRandomAccessStream> Ready;
    }
}