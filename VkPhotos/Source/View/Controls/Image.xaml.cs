using System;
using Venz.Extensions;
using Venz.Images;
using Windows.Foundation;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace VkPhotos.View.Controls
{
    public class Image: Control, IImageUriRenderer, IImageStreamRenderer
    {
        private Boolean IsImageSet => (StreamContent != null) || (UriContent != null);
        private ImageUriContent _UriContent;
        private ImageStreamContent _StreamContent;
        private Grid LayoutControl;
        private Windows.UI.Xaml.Controls.Image ImageControl;

        public static readonly DependencyProperty DecodePixelSizeProperty =
            DependencyProperty.Register("DecodePixelSize", typeof(Int32), typeof(Image), new PropertyMetadata(0));

        public Int32 DecodePixelSize
        {
            get { return (Int32)GetValue(DecodePixelSizeProperty); }
            set { SetValue(DecodePixelSizeProperty, value); }
        }

        public Object PictureRequestId { get; set; }
        public ImageUriContent UriContent { get { return _UriContent; } set { _UriContent = value; OnUriContentChanged(value); } }
        public ImageStreamContent StreamContent { get { return _StreamContent; } set { _StreamContent = value; OnStreamContentChanged(value); } }



        public Image()
        {
            DefaultStyleKey = typeof(Image);
            UniversalDependencyObject.RegisterPropertyChangedCallback(this, BackgroundProperty, nameof(Background), (sender, property) => OnBackgroundChanged(Background));
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            LayoutControl = (Grid)GetTemplateChild(nameof(LayoutControl));
            ImageControl = (Windows.UI.Xaml.Controls.Image)GetTemplateChild(nameof(ImageControl));

            if (UriContent != null)
                OnUriContentChanged(UriContent);
            else
                OnStreamContentChanged(StreamContent);
            OnBackgroundChanged(Background);
        }

        private void OnBackgroundChanged(Brush value)
        {
            if (LayoutControl == null)
                return;
            LayoutControl.Background = IsImageSet ? null : value;
        }

        private void OnUriContentChanged(ImageUriContent uriContent)
        {
            if (ImageControl == null)
                return;
            ImageControl.Source = (uriContent?.Uri == null) ? null : GetImageSource(uriContent.Uri);
            OnBackgroundChanged(Background);
        }

        private void OnStreamContentChanged(ImageStreamContent streamContent)
        {
            if (ImageControl == null)
                return;
            ImageControl.Source = (streamContent?.Stream == null) ? null : GetImageSource(streamContent.Size, streamContent.Stream);
            OnBackgroundChanged(Background);
        }

        protected ImageSource GetImageSource(Uri uri)
        {
            var bitmapImage = new BitmapImage();
            bitmapImage.UriSource = uri;
            if (DecodePixelSize > 0)
            {
                if (bitmapImage.PixelWidth < bitmapImage.PixelHeight)
                    bitmapImage.DecodePixelWidth = DecodePixelSize;
                else
                    bitmapImage.DecodePixelHeight = DecodePixelSize;
            }
            return bitmapImage;
        }

        private ImageSource GetImageSource(Size? size, IRandomAccessStream stream)
        {
            var bitmapImage = new BitmapImage();
            if (size.HasValue)
            {
                if (DecodePixelSize > 0)
                {
                    if (size.Value.IsPortrait())
                        bitmapImage.DecodePixelWidth = DecodePixelSize;
                    else
                        bitmapImage.DecodePixelHeight = DecodePixelSize;
                }
            }

            stream.Seek(0);
            bitmapImage.SetSource(stream);
            return bitmapImage;
        }
    }
}
