using System;
using VkPhotos.ViewModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace VkPhotos.View.Controls
{
    public sealed partial class ProgressIndicator: UserControl
    {
        private PageContext Context;

        public ProgressIndicator()
        {
            InitializeComponent();
            DataContextChanged += OnDataContextChanged;
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
        }

        private async void OnProgressNotificationChanged(Object sender, PageContext.ProgressNotificationUpdate notificationUpdate) => await App.Dispatcher.RunAsync(() =>
        {
            if (notificationUpdate?.IsVisible == true)
            {
                MessageControl.Text = notificationUpdate.Message ?? "";
                ProgressControl.IsIndeterminate = notificationUpdate.IsIndeterminate;
                ProgressControl.Visibility = notificationUpdate.Value.HasValue || notificationUpdate.IsIndeterminate ? Visibility.Visible : Visibility.Collapsed;
                ProgressControl.Value = notificationUpdate.Value.HasValue ? notificationUpdate.Value.Value * 100 : 0;
                Visibility = Visibility.Visible;
            }
            else
            {
                Visibility = Visibility.Collapsed;
            }
        });
    }
}
