using System;
using System.Collections.Generic;
using Venz.Extensions;
using Venz.Images;
using VkPhotos.Model;
using VkPhotos.ViewModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace VkPhotos.View
{
    public sealed partial class PhotoListPage: Venz.UI.Xaml.Page
    {
        private PhotoListContext Context;

        public PhotoListPage()
        {
            InitializeComponent();
            Context = new PhotoListContext();
            DataContext = Context;
        }

        protected override void OnNavigatedTo(NavigationEventArgs args)
        {
            base.OnNavigatedTo(args);
            Context.Initialize((args.Parameter is IEnumerable<Object>) ? (IEnumerable<Object>)args.Parameter : new List<Object>());
        }

        private async void OnItemClicked(Object sender, ItemClickEventArgs args)
        {
            var photo = (Photo)args.ClickedItem;
            var container = (Controls.Image)((ContentControl)((GridView)sender).ContainerFromItem(photo)).ContentTemplateRoot;

            if (photo != null)
            {
                var pictureView = new PictureViewContent(container, photo.LargePreview, photo);
                await pictureView.ShowAsync();
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs args)
        {
            base.OnNavigatedFrom(args);
            PictureLoader.Remove("PhotoList");
        }

        private void OnHeaderClicked(Object sender, RoutedEventArgs args)
        {
            var selectedItemIndex = Context.Items.IndexOf((a => a.Header == (String)((Button)sender).Content));
            ZoomedOutView.SelectedIndex = selectedItemIndex.HasValue ? (Int32)selectedItemIndex.Value : -1;
        }
    }
}