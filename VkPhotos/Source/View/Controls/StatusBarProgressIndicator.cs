using System;
using VkPhotos.ViewModel;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;

namespace VkPhotos.View.Controls
{
    public class StatusBarProgressIndicator: FrameworkElement
    {
        private PageContext Context;
        private StatusBar StatusBar;
        private Boolean Subscribed;

        public StatusBarProgressIndicator()
        {
            StatusBar = StatusBar.GetForCurrentView();
            DataContextChanged += OnDataContextChanged;
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }

        private void OnLoaded(Object sender, RoutedEventArgs args)
        {
            if (Context != null && !Subscribed)
            {
                Context.ProgressNotificationUpdated += OnProgressNotificationChanged;
                OnProgressNotificationChanged(Context, Context.ProgressNotification);
            }
        }

        private async void OnUnloaded(Object sender, RoutedEventArgs args)
        {
            if (Context != null)
                Context.ProgressNotificationUpdated -= OnProgressNotificationChanged;
            Subscribed = false;
            await StatusBar.ProgressIndicator.HideAsync();
        }

        private void OnDataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            if (!(args.NewValue is PageContext))
                throw new InvalidOperationException();

            if (Context != null)
            {
                Context.ProgressNotificationUpdated -= OnProgressNotificationChanged;
            }
            Context = (PageContext)args.NewValue;
            if (Context != null)
            {
                Context.ProgressNotificationUpdated += OnProgressNotificationChanged;
                OnProgressNotificationChanged(Context, Context.ProgressNotification);
            }
            Subscribed = true;
        }

        private async void OnProgressNotificationChanged(Object sender, PageContext.ProgressNotificationUpdate notificationUpdate) => await App.Dispatcher.RunAsync(async () =>
        {
            if (notificationUpdate?.IsVisible == true)
            {
                StatusBar.ProgressIndicator.Text = notificationUpdate.Message ?? "";
                if (notificationUpdate.IsIndeterminate)
                    StatusBar.ProgressIndicator.ProgressValue = null;
                else
                    StatusBar.ProgressIndicator.ProgressValue = notificationUpdate.Value.HasValue ? notificationUpdate.Value.Value : 0;
                await StatusBar.ProgressIndicator.ShowAsync().AsTask().ConfigureAwait(false);
            }
            else
            {
                await StatusBar.ProgressIndicator.HideAsync().AsTask().ConfigureAwait(false);
            }
        });
    }
}
