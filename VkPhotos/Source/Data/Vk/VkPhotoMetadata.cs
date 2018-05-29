using System;
using System.Collections.Generic;
using Venz.Extensions;
using Windows.Data.Json;

namespace VkPhotos.Data.Vk
{
    public class VkPhotoMetadata: IPhotoMetadata
    {
        public UInt32? Id { get; private set; }
        public OwnerId Owner { get; private set; }
        public Double? Latitude { get; private set; }
        public Double? Longitude { get; private set; }
        public DateTime? Date { get; private set; }
        public String Text { get; private set; }
        public IReadOnlyCollection<IPhoto> Photos { get; private set; }



        private VkPhotoMetadata() { }

        public VkPhotoMetadata(UInt32? id, OwnerId owner, Double? latitude, Double? longitude, DateTime? date, String text, IReadOnlyCollection<IPhoto> photos)
        {
            Id = id;
            Owner = owner;
            Latitude = latitude;
            Longitude = longitude;
            Date = date;
            Text = text;
            Photos = photos;
        }

        public static IPhotoMetadata Create(JsonObject value)
        {
            var photos = new List<IPhoto>();
            if (value.ContainsKey("sizes") && (value["sizes"].ValueType == JsonValueType.Array))
            {
                foreach (var item in value["sizes"].GetArray())
                {
                    var photo = VkPhoto.Create(item.GetObject());
                    if (photo != null)
                        photos.Add(photo);
                }
            }

            var instance = new VkPhotoMetadata();
            if (value.ContainsKey("id"))
                instance.Id = (UInt32)value["id"].GetNumber();
            if (value.ContainsKey("owner_id"))
            {
                var owner = value["owner_id"].GetNumber();
                instance.Owner = (owner < 0) ? OwnerId.Create((UInt32)Math.Abs(owner), OwnerId.OwnerType.Group) : OwnerId.Create((UInt32)owner, OwnerId.OwnerType.User);
            }
            if (value.ContainsKey("lat"))
                instance.Latitude = value["lat"].GetNumber();
            if (value.ContainsKey("long"))
                instance.Longitude = value["long"].GetNumber();
            if (value.ContainsKey("date"))
                instance.Date = DateTimeExtensions.FromUnixTimestamp((Int32)value["date"].GetNumber());
            if (value.ContainsKey("text"))
                instance.Text = value["text"].GetString();
            instance.Photos = photos;
            return instance;
        }
    }
}
