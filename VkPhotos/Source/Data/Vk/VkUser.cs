using System;
using Windows.Data.Json;

namespace VkPhotos.Data.Vk
{
    public class VkUser: IUser
    {
        public UInt32 Id { get; private set; }
        public String FirstName { get; private set; }
        public String LastName { get; private set; }
        public Uri Photo { get; private set; }



        private VkUser() { }

        public static IUser Create(JsonObject value)
        {
            var instance = new VkUser();
            if (value.ContainsKey("id"))
                instance.Id = (UInt32)value["id"].GetNumber();
            if (value.ContainsKey("first_name"))
                instance.FirstName = value["first_name"].GetString();
            if (value.ContainsKey("last_name"))
                instance.LastName = value["last_name"].GetString();
            if (value.ContainsKey("photo_100"))
                instance.Photo = new Uri(value["photo_100"].GetString(), UriKind.Absolute);
            return instance;
        }
    }
}
