using System;
using Venz.UI.Xaml;
using Windows.UI.Xaml;

namespace VkPhotos.View.Converters
{
    public class BooleanToVisibility: OneWayValueConverter<Boolean>
    {
        public Visibility TrueValue { get; set; }
        public Visibility FalseValue { get; set; }
        protected override Object Convert(Boolean value) => value ? TrueValue : FalseValue;
    }

    public class BooleanToDouble: OneWayValueConverter<Boolean>
    {
        public Double TrueValue { get; set; }
        public Double FalseValue { get; set; }
        protected override Object Convert(Boolean value) => value ? TrueValue : FalseValue;
    }

    public class UIntToBytesSize: OneWayValueConverter<UInt64>
    {
        public static String Get(UInt64 value) => (String)new UIntToBytesSize().Convert(value, null, null, null);
        protected override Object Convert(UInt64 value)
        {
            if (value >= 1000 * 1000 * 1000)
                return $"{((Double)value / 1024 / 1024 / 1024).ToString("F1")} {Strings.Text_GigaBytes}";
            else if (value >= 1000 * 1000)
                return $"{((Double)value / 1024 / 1024).ToString("F1")} {Strings.Text_MegaBytes}";
            else if (value >= 1000)
                return $"{((Double)value / 1024).ToString("F1")} {Strings.Text_KiloBytes}";
            else
                return $"{value} {Strings.Text_Bytes}";
        }
    }
}