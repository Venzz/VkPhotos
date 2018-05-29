using System;
using Venz.UI.Xaml;

namespace VkPhotos
{
    public class Settings: ApplicationSettings
    {
        public Double MapLatitude
        {
            get { return Get<Double>("MapLatitude", 0.0); }
            set { Set("MapLatitude", value); }
        }

        public Double MapLongitude
        {
            get { return Get<Double>("MapLongitude", 0.0); }
            set { Set("MapLongitude", value); }
        }

        public Double MapZoomLevel
        {
            get { return Get<Double>("MapZoomLevel", 0); }
            set { Set("MapZoomLevel", value); }
        }

        public Boolean AutomaticSearch
        {
            get { return Get<Boolean>("AutomaticSearch", true); }
            set { Set("AutomaticSearch", value); }
        }

        public Boolean IsPastPeriodEnabled
        {
            get { return Get<Boolean>("IsPastPeriodEnabled", true); }
            set { Set("IsPastPeriodEnabled", value); }
        }

        public PastPeriod PastPeriod
        {
            get { return (PastPeriod)Get<Byte>("PastPeriod", (Byte)PastPeriod.Day); }
            set { Set("PastPeriod", (Byte)value); }
        }

        public Boolean IsPeriodEnabled
        {
            get { return Get<Boolean>("IsPeriodEnabled", true); }
            set { Set("IsPeriodEnabled", value); }
        }

        public DateTime FromDate
        {
            get { return Get("FromDate", new DateTime(2016, 1, 1)); }
            set { Set("FromDate", value); }
        }

        public DateTime ToDate
        {
            get { return Get("ToDate", DateTime.Now); }
            set { Set("ToDate", value); }
        }

        public Boolean IsCachingEnabled
        {
            get { return Get<Boolean>("IsCachingEnabled", true); }
            set { Set("IsCachingEnabled", value); }
        }

        public UInt32 AppLaunchAmount
        {
            get { return Get<UInt32>("AppLaunchAmount", 0); }
            set { Set("AppLaunchAmount", value); }
        }

        public Boolean AppReviewComplete
        {
            get { return Get<Boolean>("AppReviewComplete", false); }
            set { Set("AppReviewComplete", value); }
        }



        public UInt32? UserId
        {
            get { return Get<UInt32?>("UserId", null); }
            set { Set("UserId", value); }
        }

        public String FirstName
        {
            get { return Get<String>("FirstName", null); }
            set { Set("FirstName", value); }
        }

        public String LastName
        {
            get { return Get<String>("LastName", null); }
            set { Set("LastName", value); }
        }

        public String AccessToken
        {
            get { return Get<String>("AccessToken", null); }
            set { Set("AccessToken", value); }
        }
    }
}
