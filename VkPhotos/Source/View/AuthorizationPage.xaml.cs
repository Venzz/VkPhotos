using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Venz.Extensions;
using Venz.UI.Xaml;
using VkPhotos.Data.Vk;
using VkPhotos.ViewModel;
using Windows.System;
using Windows.Web.Http.Filters;

namespace VkPhotos.View
{
    public sealed partial class AuthorizationPage: Page
    {
        private PageContext Context = new PageContext();
        private IList<PageContext.ProgressNotificationId> NotificationIds = new List<PageContext.ProgressNotificationId>();

        public AuthorizationPage()
        {
            InitializeComponent();
            DataContext = Context;
            WebView.NavigationStarting += OnWebNavigationStarting;
            WebView.NavigationCompleted += OnWebNavigationCompleted;
            WebView.NavigationFailed += OnWebNavigationFailed;
        }

        private void OnWebNavigationStarting(Windows.UI.Xaml.Controls.WebView sender, Windows.UI.Xaml.Controls.WebViewNavigationStartingEventArgs args)
        {
            NotificationIds.Add(Context.ShowProgress(Strings.ProgressIndicator_Loading));
        }

        protected override async void SetState(FrameNavigation.Parameter navigationParameter, FrameNavigation.Parameter stateParameter)
        {
            base.SetState(navigationParameter, stateParameter);
            if (!App.Model.VkConnectAuthorizationFailed)
            {
                var vkConnectUri = VkAuthorization.GetVkConnectUri();
                await Launcher.LaunchUriAsync(vkConnectUri);
            }

            var protocolFilter = new HttpBaseProtocolFilter();
            foreach (var cookie in protocolFilter.CookieManager.GetCookies(new Uri("https://oauth.vk.com")))
                protocolFilter.CookieManager.DeleteCookie(cookie);
            WebView.Source = VkAuthorization.GetOAuthUri();
        }

        private async void OnWebNavigationCompleted(Windows.UI.Xaml.Controls.WebView sender, Windows.UI.Xaml.Controls.WebViewNavigationCompletedEventArgs args)
        {
            while (NotificationIds.Count > 0)
            {
                Context.HideProgress(NotificationIds[0]);
                NotificationIds.RemoveAt(0);
            }
            if (!args.Uri.ToString().StartsWith(VkAuthorization.GetRedirectUrl()))
            {
                return;
            }
            if (await TryFinishAuthorizationAsync(args.Uri))
            {
                App.Model.VkConnectAuthorizationFailed = true;
            }
        }

        public async Task<Boolean> TryFinishAuthorizationAsync(Uri uri)
        {
            var accessToken = uri.ToString().Between("access_token=", false, "&", true);
            var userId = uri.ToString().Between("user_id=", false, "&", true);
            if (String.IsNullOrWhiteSpace(accessToken) || String.IsNullOrWhiteSpace(userId))
            {
                App.Diagnostics.Error.Log("AuthorizationPage.TryFinishAuthorizationAsync", "Uri", uri.ToString());
                return false;
            }

            await App.Model.Users.SignInAsync(Convert.ToUInt32(userId), accessToken);
            Frame.GoBack();
            return true;
        }

        private void OnWebNavigationFailed(Object sender, Windows.UI.Xaml.Controls.WebViewNavigationFailedEventArgs args)
        {
            while (NotificationIds.Count > 0)
            {
                Context.HideProgress(NotificationIds[0]);
                NotificationIds.RemoveAt(0);
            }
        }
    }
}
