using System;
using Venz.Images;
using VkPhotos.Data;

namespace VkPhotos.Model
{
    public class User
    {
        public UInt32 Id { get; }
        public String Name { get; }
        public Picture Photo { get; }
        public String AccessToken { get; }

        public User(UInt32 id, String firstName, String lastName, String accessToken)
        {
            Id = id;
            Name = $"{firstName} {lastName}";
            AccessToken = accessToken;
            Photo = new CachedUserPicture();
        }

        public User(IUser data, String accessToken)
        {
            Id = data.Id;
            Name = $"{data.FirstName} {data.LastName}";
            AccessToken = accessToken;
            Photo = new UserPicture(data.Id, data.Photo);
        }
    }
}
