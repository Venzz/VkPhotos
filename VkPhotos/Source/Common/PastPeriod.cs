using System;

namespace VkPhotos
{
    public enum PastPeriod
    {
        Hour,
        Day,
        Month,
        Year,
    }

    public static class PastPeriodExtensions
    {
        public static Tuple<DateTime, DateTime> GetDates(this PastPeriod period)
        {
            var now = DateTime.UtcNow;
            var endDate = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0, DateTimeKind.Utc);
            var startDate = endDate.AddDays(-1);
            switch (App.Settings.PastPeriod)
            {
                case PastPeriod.Hour:
                    startDate = endDate.AddHours(-1);
                    break;
                case PastPeriod.Day:
                    startDate = endDate.AddDays(-1);
                    break;
                case PastPeriod.Month:
                    startDate = endDate.AddMonths(-1);
                    break;
                case PastPeriod.Year:
                    startDate = endDate.AddYears(-1);
                    break;
                default:
                    throw new NotSupportedException($"Value of type {period} isn't supported.");
            }
            return new Tuple<DateTime, DateTime>(startDate, endDate);
        }
    }
}
