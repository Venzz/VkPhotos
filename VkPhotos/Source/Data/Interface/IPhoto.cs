using System;
using Windows.Foundation;

namespace VkPhotos.Data
{
    public interface IPhoto
    {
        Size Size { get; }
        Uri Source { get; }
        String Type { get; }
    }
}
