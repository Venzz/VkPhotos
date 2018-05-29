using System;
using VkPhotos.Data;
using Windows.Foundation;

namespace VkPhotos.Screenshots
{
    public class ScreenshotsDataPhoto: IPhoto
    {
        public Size Size { get; }
        public Uri Source { get; }
        public String Type { get; }



        private ScreenshotsDataPhoto() { }

        public ScreenshotsDataPhoto(Size size, Uri source, String type)
        {
            Size = size;
            Source = source;
            Type = type;
        }
    }
}
