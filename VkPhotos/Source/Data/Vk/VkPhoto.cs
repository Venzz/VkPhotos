using System;
using Windows.Data.Json;
using Windows.Foundation;
using Venz.Extensions;

namespace VkPhotos.Data.Vk
{
    public class VkPhoto: IPhoto
    {
        public Size Size { get; private set; }
        public Uri Source { get; private set; }
        public String Type { get; private set; }



        private VkPhoto() { }

        public VkPhoto(Size size, Uri source, String type)
        {
            Size = size;
            Source = source;
            Type = type;
        }

        public static IPhoto Create(JsonObject value)
        {
            if (!value.ContainsKey("width") || !value.ContainsKey("height") || !value.ContainsKey("src") || !value.ContainsKey("type"))
                return null;

            var instance = new VkPhoto();
            instance.Size = new Size((UInt16)value["width"].GetNumber(), (UInt16)value["height"].GetNumber());
            if (!instance.Size.IsVisible())
                return null;
            instance.Source = new Uri(value["src"].GetString());
            instance.Type = value["type"].GetString();
            return instance;
        }

        public override String ToString() => $"{Type}, {Size.Width}x{Size.Height}, {Source}";
    }
}
