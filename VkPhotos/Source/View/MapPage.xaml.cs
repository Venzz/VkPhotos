using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Venz.Extensions;
using Venz.UI.Xaml;
using Venz.UI.Xaml.Common;
using Venz.Windows;
using VkPhotos.Model;
using VkPhotos.Model.Map;
using VkPhotos.View.Controls;
using VkPhotos.ViewModel;
using Windows.ApplicationModel.DataTransfer;
using Windows.Devices.Geolocation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Navigation;

namespace VkPhotos.View
{
    public sealed partial class MapPage: Venz.UI.Xaml.Page
    {
        private Int32 SelectedMenuIndex;
        private MapContext Context = new MapContext();
        private MapControlEventSampler MapControlEventSampler;
        private MapObjects<MapPhoto, MapCluster> MapObjects;

        public MapPage(): base(NavigationCacheMode.Required)
        {
            InitializeComponent();
            DataContext = Context;
            Context.PhotosAdded += OnPhotosAdded;
            Context.AuthorizationFailed += OnAuthorizationFailed;

            var mapView = App.Model.MapView.Get();
            Map.Center = mapView.Location.ToGeopoint();
            Map.ZoomLevel = mapView.ZoomLevel;
            Map.MapServiceToken = PrivateData.MapServiceToken;

            MapSettings.SetContext(Map);
            SizeChanged += OnSizeChanged;

            DataTransferManager.GetForCurrentView().DataRequested += OnShareDataRequested;
        }

        protected override async void SetState(FrameNavigation.Parameter navigationParameter, FrameNavigation.Parameter stateParameter)
        {
            base.SetState(navigationParameter, stateParameter);
            if (navigationParameter.Get() == "share")
            {
                PrimaryMenu.SelectedIndex = 3;
            }
            else
            {
                PrimaryMenu.Items.Remove(SharedLinkMenuItem);
                if (!App.Model.MapView.Available)
                {
                    var mapView = await Task.Run(async () => await App.Model.MapView.GetAsync().ConfigureAwait(false));
                    if (!App.Model.MapView.Available && (mapView != null))
                    {
                        Map.Center = mapView.Location.ToGeopoint();
                        Map.ZoomLevel = mapView.ZoomLevel;
                    }
                }
                PrimaryMenu.SelectedIndex = 1;
            }

            await Context.InitializeAsync();
            await TryAskForReviewAsync();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs args)
        {
            base.OnNavigatedTo(args);
            if (args.NavigationMode == NavigationMode.Back)
            {
                await Context.UpdateStateAsync();
                if ((App.Model.MapView.SharedLinkSettings != null) && !PrimaryMenu.Items.Contains(SharedLinkMenuItem))
                    PrimaryMenu.Items.Add(SharedLinkMenuItem);
            }
        }

        private async void OnSizeChanged(Object sender, SizeChangedEventArgs args)
        {
            var latitudeTiles = (UInt16)Math.Max(1, Math.Ceiling(args.NewSize.Height / 400) - 1);
            var longitudeTiles = (UInt16)Math.Max(1, Math.Ceiling(args.NewSize.Width / 400) - 1);

            if (MapObjects == null)
            {
                MapObjects = new MapObjects<MapPhoto, MapCluster>(Map, longitudeTiles, latitudeTiles);
                MapObjects.ObjectTapped += OnObjectTapped;
                MapObjects.ClusterTapped += OnClusterTapped;

                MapControlEventSampler = new MapControlEventSampler(Map, TimeSpan.FromMilliseconds(2000));
                MapControlEventSampler.MapViewChanged += OnMapViewChanged;

                if (PrimaryMenu.SelectedIndex == 1)
                {
                    var mapViewSettings = App.Model.MapView.Get();
                    if (mapViewSettings != null)
                        await Context.StartProgressiveSearchAsync(mapViewSettings, PhotoTag.Public);
                }
            }
            else
            {
                MapObjects.Apply(longitudeTiles, latitudeTiles);
            }
        }

        public void SelectSharedLinkMenuItem()
        {
            if (!PrimaryMenu.Items.Contains(SharedLinkMenuItem))
                PrimaryMenu.Items.Add(SharedLinkMenuItem);
            PrimaryMenu.SelectedIndex = 3;
        }

        private async void OnMapViewChanged(MapControlEventSampler.MapViewChangedEventArgs args)
        {
            if ((args.Center.Latitude != 0) || (args.Center.Latitude != 0))
                App.Model.MapView.Store(args.Center, args.ZoomLevel);
            if (App.Settings.AutomaticSearch && (SelectedMenuIndex == 1))
                await Context.StartProgressiveSearchAsync(App.Model.MapView.Get(), PhotoTag.Public);
        }

        private void OnPhotosAdded(MapContext sender, IReadOnlyCollection<Photo> photos)
        {
            foreach (var photo in photos)
                MapObjects.Add(new GeoObject(photo.Location, photo));
            MapObjects.TriggerCurrentViewRerendering();
        }

        private async void OnObjectTapped(MapPhoto sender, Object context)
        {
            var photo = (Photo)context;
            if (photo != null)
            {
                var pictureView = new PictureViewContent(sender.GetImageControl(), photo.Preview, photo);
                await pictureView.ShowAsync();
            }
        }

        private void OnClusterTapped(MapCluster sender, Object args) => Frame.Navigate(typeof(PhotoListPage), sender.Objects);

        private void OnSettingsButtonClicked(Object sender, RoutedEventArgs args) => Navigation.Navigate(typeof(SettingsPage));

        private async void OnUserLocationButtonClicked(Object sender, RoutedEventArgs args)
        {
            var mapView = await App.Model.MapView.GetCurrentAsync();
            await Map.TrySetViewAsync(mapView.Location.ToGeopoint(), mapView.ZoomLevel);
        }

        private async void OnAuthorizationFailed(Object sender, EventArgs args)
        {
            PrimaryMenu.SelectedIndex = 1;
            await Context.SignOutAsync();
            await App.Dispatcher.RunAsync(() => Navigation.Navigate(typeof(AuthorizationPage)));
        }

        private async void OnPrimaryMenuSelectionChanged(Object sender, SelectionChangedEventArgs args)
        {
            if (SelectedMenuIndex == PrimaryMenu.SelectedIndex)
                return;

            switch (PrimaryMenu.SelectedIndex)
            {
                case 0:
                    SplitView.IsPaneOpen = !SplitView.IsPaneOpen;
                    PrimaryMenu.SelectedItem = (args.RemovedItems.Count > 0) ? args.RemovedItems[0] : null;
                    break;
                case 1:
                    SelectedMenuIndex = 1;
                    ShareButton.Visibility = Visibility.Visible;
                    SearchButton.Visibility = Visibility.Visible;
                    if (MapObjects != null)
                        MapObjects.SetFilter(new PublicPhotosFilter());
                    break;
                case 2:
                    if (Context.IsUserAuthorized)
                    {
                        SelectedMenuIndex = 2;
                        SearchButton.Visibility = Visibility.Collapsed;
                        ShareButton.Visibility = Visibility.Collapsed;
                        if (MapObjects != null)
                            MapObjects.SetFilter(new PersonalPhotosFilter());
                        await Context.LoadPersonalPhotosAsync().ConfigureAwait(false);
                    }
                    else
                    {
                        PrimaryMenu.SelectedItem = (args.RemovedItems.Count > 0) ? args.RemovedItems[0] : null;
                    }
                    break;
                case 3:
                    SelectedMenuIndex = 3;
                    ShareButton.Visibility = Visibility.Collapsed;
                    SearchButton.Visibility = Visibility.Visible;
                    if (MapObjects != null)
                        MapObjects.SetFilter(new SharedLinkPhotosFilter());

                    await Map.TrySetViewAsync(App.Model.MapView.SharedLinkSettings.Location.ToGeopoint(), App.Model.MapView.SharedLinkSettings.ZoomLevel);
                    await Context.StartProgressiveSearchAsync(App.Model.MapView.SharedLinkSettings, PhotoTag.SharedLink);
                    break;
            }
        }

        private void OnPrimaryMenuHeaderIconTapped(Object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs args) => SplitView.IsPaneOpen = !SplitView.IsPaneOpen;

        private async void OnSecondaryMenuItemClicked(Object sender, ItemClickEventArgs args)
        {
            if (Context.IsUserAuthorized)
            {
                var result = await MessageDialog.ConfirmAsync(Strings.Message_SignOutWarning_Header, Strings.Message_SignOutWarning_Text, Strings.Message_YesButton, Strings.Message_NoButton);
                if (result == Strings.Message_YesButton)
                {
                    await Context.SignOutAsync();
                    if (PrimaryMenu.SelectedIndex == 2)
                        PrimaryMenu.SelectedIndex = 1;
                }
            }
            else
            {
                Navigation.Navigate(typeof(AuthorizationPage));
            }
        }

        private void OnSecondaryMenuSelectionChanged(Object sender, SelectionChangedEventArgs args)
        {
            if (PrimaryMenu.SelectedIndex == 0)
            {
                SplitView.IsPaneOpen = !SplitView.IsPaneOpen;
                PrimaryMenu.SelectedItem = (args.RemovedItems.Count > 0) ? args.RemovedItems[0] : null;
            }
        }

        private async void OnLocationSettingsButtonTapped(Object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs args) => await ServiceLauncher.Settings.OpenLocationAsync();

        private async void OnZoomInButtonClicked(Object sender, RoutedEventArgs args) => await Map.TryZoomInAsync();

        private async void OnZoomOutButtonClicked(Object sender, RoutedEventArgs args) => await Map.TryZoomOutAsync();

        private async void OnSearchButtonClicked(Object sender, RoutedEventArgs args)
        {
            switch (PrimaryMenu.SelectedIndex)
            {
                case 1:
                    await Context.StartProgressiveSearchAsync(App.Model.MapView.Get(), PhotoTag.Public);
                    break;
                case 3:
                    await Context.StartProgressiveSearchAsync(App.Model.MapView.SharedLinkSettings, PhotoTag.SharedLink);
                    break;
            }
        }

        private void OnAboutButtonClicked(Object sender, RoutedEventArgs args)
        {
            var parameter = new FrameNavigation.Parameter("title", Constants.ApplicationTitle).Add("version", Constants.ApplicationVersion);
            AboutPage.ExtendedContent = new ExtendedAboutPageContent();
            Navigation.Navigate(typeof(AboutPage), parameter);
        }

        private void OnShareMenuItemClicked(Object sender, RoutedEventArgs args) => DataTransferManager.ShowShareUI();

        private void OnShareDataRequested(DataTransferManager sender, DataRequestedEventArgs args)
        {
            args.Request.Data.Properties.Title = String.Format(Strings.Text_ShareMapView_Title, Constants.ApplicationTitle);
            args.Request.Data.Properties.Description = Strings.Text_ShareMapView_Description;
            args.Request.Data.SetWebLink(new Uri($"http://vk-photos.appspot.com/share/v1?id={CreateSettingsId()}", UriKind.Absolute));
        }

        private String CreateSettingsId()
        {
            var from = App.Settings.IsPeriodEnabled ? App.Settings.FromDate.ToUnixTimestamp() : App.Settings.PastPeriod.GetDates().Item1.ToUnixTimestamp();
            var to = App.Settings.IsPeriodEnabled ? App.Settings.ToDate.ToUnixTimestamp() : DateTime.UtcNow.ToUnixTimestamp();
            var parameters = new String[] { from.ToString(), to.ToString(), MapControlEventSampler.Center.Latitude.ToString(CultureInfo.InvariantCulture), MapControlEventSampler.Center.Longitude.ToString(CultureInfo.InvariantCulture), MapControlEventSampler.ZoomLevel.ToString() };
            return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(String.Join("|", parameters)));
        }

        private async Task TryAskForReviewAsync()
        {
            App.Settings.AppLaunchAmount++;
            if (!App.Settings.AppReviewComplete && (App.Settings.AppLaunchAmount % 5 == 0))
            {
                if (await MessageDialog.ConfirmAsync(Strings.Message_Review_Header, Strings.Message_Review_Text, Strings.Message_YesButton, Strings.Message_NoButton) == Strings.Message_YesButton)
                {
                    await ServiceLauncher.ShowReviewRequestAsync(Constants.ApplicationId);
                    App.Settings.AppReviewComplete = true;
                }
            }
        }



        private class PublicPhotosFilter: IMapObjectsFilter
        {
            public Boolean IsAccepted(GeoObject geoObject)
            {
                var photo = (Photo)geoObject.Value;
                return photo.Tag.HasFlag(PhotoTag.Public);
            }
        }

        private class PersonalPhotosFilter: IMapObjectsFilter
        {
            public Boolean IsAccepted(GeoObject geoObject)
            {
                var photo = (Photo)geoObject.Value;
                return photo.Owner.Value == App.Settings.UserId.Value;
            }
        }

        private class SharedLinkPhotosFilter: IMapObjectsFilter
        {
            public Boolean IsAccepted(GeoObject geoObject)
            {
                var photo = (Photo)geoObject.Value;
                return photo.Tag.HasFlag(PhotoTag.SharedLink);
            }
        }
    }
}
