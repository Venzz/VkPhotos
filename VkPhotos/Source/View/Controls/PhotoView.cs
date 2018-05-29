using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace VkPhotos.View.Controls
{
    public class PhotoView: GridView
    {
        public static readonly DependencyProperty PhotoSizeProperty =
            DependencyProperty.Register("PhotoSize", typeof(Int32), typeof(PhotoView), new PropertyMetadata(140));

        public Int32 PhotoSize
        {
            get { return (Int32)GetValue(PhotoSizeProperty); }
            set { SetValue(PhotoSizeProperty, value); }
        }



        public PhotoView()
        {
            SizeChanged += OnSizeChanged;
        }

        private void OnSizeChanged(Object sender, SizeChangedEventArgs args)
        {
            var panel = (ItemsWrapGrid)ItemsPanelRoot;
            panel.ItemWidth = args.NewSize.Width / ((UInt32)args.NewSize.Width / PhotoSize);
            panel.ItemHeight = panel.ItemWidth;
        }
    }
}
