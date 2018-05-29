using System;
using Venz.Images;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace VkPhotos.View.Controls
{
    public class CircleImage: Control, IImageStreamRenderer
    {
        private Boolean IsImageSet => (StreamContent != null);
        private ImageStreamContent _StreamContent;
        private Grid LayoutControl;
        private ImageBrush ImageControl;

        public Object PictureRequestId { get; set; }
        public ImageStreamContent StreamContent { get { return _StreamContent; } set { _StreamContent = value; OnStreamContentChanged(value); } }



        public CircleImage()
        {
            DefaultStyleKey = typeof(CircleImage);
            RegisterPropertyChangedCallback(BackgroundProperty, (sender, property) => OnBackgroundChanged(Background));
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            LayoutControl = (Grid)GetTemplateChild(nameof(LayoutControl));
            ImageControl = (ImageBrush)GetTemplateChild(nameof(ImageControl));
            OnStreamContentChanged(StreamContent);
            OnBackgroundChanged(Background);
        }

        private void OnBackgroundChanged(Brush value)
        {
            if (LayoutControl == null)
                return;
            LayoutControl.Background = IsImageSet ? null : value;
        }

        private void OnStreamContentChanged(ImageStreamContent streamContent)
        {
            if (ImageControl == null)
                return;

            ImageControl.ImageSource = (streamContent?.Stream == null) ? null : GetImageSource(streamContent.Stream);
            OnBackgroundChanged(Background);
        }

        private ImageSource GetImageSource(IRandomAccessStream stream)
        {
            var bitmapImage = new BitmapImage();
            stream.Seek(0);
            bitmapImage.SetSource(stream);
            return bitmapImage;
        }
    }
}
