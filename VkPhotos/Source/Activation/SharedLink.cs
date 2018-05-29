using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Venz.Extensions;
using Venz.UI.Xaml;
using Venz.Windows;
using VkPhotos.Model;
using VkPhotos.View;
using Windows.UI.Xaml;

namespace VkPhotos
{
    public static class SharedLink
    {
        public static MapViewSettings Parse(Uri sharedLink)
        {
            try
            {
                var sharedLinkId = sharedLink.Segments.Last();
                var parametersString = Encoding.UTF8.GetString(Convert.FromBase64String(sharedLinkId));
                var parameters = parametersString.Split('|');

                var fromDate = DateTimeExtensions.FromUnixTimestamp(Convert.ToInt32(parameters[0]));
                var toDate = DateTimeExtensions.FromUnixTimestamp(Convert.ToInt32(parameters[1]));
                var latitude = Convert.ToDouble(parameters[2], CultureInfo.InvariantCulture);
                var longitude = Convert.ToDouble(parameters[3], CultureInfo.InvariantCulture);
                var zoomLevel = Convert.ToByte(parameters[4]);
                return new MapViewSettings(fromDate, toDate, latitude, longitude, zoomLevel);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static Task ShowLinkCreationFailedDialogAsync() => MessageDialog.ShowAsync(Strings.Message_SharedLinkCreationFailed_Header, Strings.Message_SharedLinkCreationFailed_Text);

        public static async Task TryApplySettingsAsync()
        {
            var frame = (Frame)Window.Current.Content;
            if (frame.Content is MapPage)
            {
                ((MapPage)frame.Content).SelectSharedLinkMenuItem();
            }
            else
            {
                var result = await MessageDialog.ConfirmAsync(Strings.Message_SharedLinkNavigationConfirmation_Header, Strings.Message_SharedLinkNavigationConfirmation_Text,
                        Strings.Message_YesButton, Strings.Message_NoButton);
                if (result == Strings.Message_NoButton)
                {

                }
                else
                {
                    while (!(frame.Content is MapPage) && frame.CanGoBack)
                        frame.GoBack();
                    ((MapPage)frame.Content).SelectSharedLinkMenuItem();
                }
            }
        }
    }
}
