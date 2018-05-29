using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace VkPhotos.View.Controls
{
    public sealed partial class DetailItem: ContentControl
    {
        public static readonly DependencyProperty HeaderProperty =
            DependencyProperty.Register("Header", typeof(String), typeof(DetailItem), new PropertyMetadata("Title"));

        public String Header
        {
            get { return (String)GetValue(HeaderProperty); }
            set { SetValue(HeaderProperty, value); }
        }

        public DetailItem()
        {
            DefaultStyleKey = typeof(DetailItem);
        }
    }
}
