using System;
using System.Globalization;
using Windows.ApplicationModel.Resources;

namespace VkPhotos
{
    public static class Strings
    {
        private static ResourceLoader ResourceLoader = new ResourceLoader();

        public static String ProgressIndicator_SearchingPhotos => ResourceLoader.GetString("ProgressIndicator_SearchingPhotos");
        public static String ProgressIndicator_SearchingPhotosResult_NoPhotos => ResourceLoader.GetString("ProgressIndicator_SearchingPhotosResult_NoPhotos");
        public static String ProgressIndicator_SearchingPhotosResult_OnePhoto => ResourceLoader.GetString("ProgressIndicator_SearchingPhotosResult_OnePhoto");
        public static String ProgressIndicator_SearchingPhotosResult_ManyPhotos => ResourceLoader.GetString("ProgressIndicator_SearchingPhotosResult_ManyPhotos");
        public static String ProgressIndicator_SearchingPhotosResult_Error => ResourceLoader.GetString("ProgressIndicator_SearchingPhotosResult_Error");
        public static String ProgressIndicator_GettingPhotos => ResourceLoader.GetString("ProgressIndicator_GettingPhotos");
        public static String ProgressIndicator_Loading => ResourceLoader.GetString("ProgressIndicator_Loading");

        public static String Page_PhotoList_Header => ResourceLoader.GetString("Page_PhotoList_Header");

        public static String Message_SignOutWarning_Text => ResourceLoader.GetString("Message_SignOutWarning_Text");
        public static String Message_SignOutWarning_Header => ResourceLoader.GetString("Message_SignOutWarning_Header");
        public static String Message_SharedLinkCreationFailed_Header => ResourceLoader.GetString("Message_SharedLinkCreationFailed_Header");
        public static String Message_SharedLinkCreationFailed_Text => ResourceLoader.GetString("Message_SharedLinkCreationFailed_Text");
        public static String Message_SharedLinkNavigationConfirmation_Header => ResourceLoader.GetString("Message_SharedLinkNavigationConfirmation_Header");
        public static String Message_SharedLinkNavigationConfirmation_Text => ResourceLoader.GetString("Message_SharedLinkNavigationConfirmation_Text");
        public static String Message_Review_Header => ResourceLoader.GetString("Message_Review_Header");
        public static String Message_Review_Text => ResourceLoader.GetString("Message_Review_Text");
        public static String Message_YesButton => ResourceLoader.GetString("Message_YesButton");
        public static String Message_NoButton => ResourceLoader.GetString("Message_NoButton");

        public static String Text_LocalFileFolderSize_Empty => ResourceLoader.GetString("Text_LocalFileFolderSize_Empty");
        public static String Text_LocalFileFolderSize => ResourceLoader.GetString("Text_LocalFileFolderSize");
        public static String Text_GigaBytes => ResourceLoader.GetString("Text_GigaBytes");
        public static String Text_MegaBytes => ResourceLoader.GetString("Text_MegaBytes");
        public static String Text_KiloBytes => ResourceLoader.GetString("Text_KiloBytes");
        public static String Text_Bytes => ResourceLoader.GetString("Text_Bytes");
        public static String Text_ShareMapView_Title => ResourceLoader.GetString("Text_ShareMapView_Title");
        public static String Text_ShareMapView_Description => ResourceLoader.GetString("Text_ShareMapView_Description");



        public static String GetProgressIndicatorSearchingPhotosResult(Int32 amount)
        {
            if (CultureInfo.CurrentCulture.Name == "ru-RU")
            {
                if (amount == 0)
                    return ProgressIndicator_SearchingPhotosResult_NoPhotos;
                else if ((amount == 1) || (amount > 20) && (amount % 10 == 1) && (amount % 100 != 11))
                    return String.Format(ProgressIndicator_SearchingPhotosResult_OnePhoto, amount);
                else
                    return String.Format(ProgressIndicator_SearchingPhotosResult_ManyPhotos, amount);
            }
            else
            {
                if (amount == 0)
                    return ProgressIndicator_SearchingPhotosResult_NoPhotos;
                else if (amount == 1)
                    return String.Format(ProgressIndicator_SearchingPhotosResult_OnePhoto, amount);
                else
                    return String.Format(ProgressIndicator_SearchingPhotosResult_ManyPhotos, amount);
            }
        }

        public static String GetTitle(PastPeriod value)
        {
            switch (value)
            {
                case PastPeriod.Hour:
                    return ResourceLoader.GetString("Text_Hour");
                case PastPeriod.Day:
                    return ResourceLoader.GetString("Text_Day");
                case PastPeriod.Month:
                    return ResourceLoader.GetString("Text_Month");
                case PastPeriod.Year:
                    return ResourceLoader.GetString("Text_Year");
                default:
                    throw new NotSupportedException($"Value of type {value} isn't supported.");
            }
        }
    }
}
