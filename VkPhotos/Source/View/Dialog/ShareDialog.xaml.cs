using System;
using Windows.ApplicationModel.DataTransfer;
using Windows.UI.Xaml.Controls;

namespace VkPhotos.View
{
    public sealed partial class ShareDialog: ContentDialog
    {
        public ShareDialog(String link)
        {
            InitializeComponent();
            SharedLink.Text = link;
        }

        private void OnCopyButtonClicked(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            var package = new DataPackage();
            package.SetText(SharedLink.Text);
            Clipboard.SetContent(package);
        }
    }
}
