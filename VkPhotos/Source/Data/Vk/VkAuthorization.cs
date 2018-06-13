using System;

namespace VkPhotos.Data.Vk
{
    public class VkAuthorization
    {
        public static String GetRedirectUrl() => "https://oauth.vk.com/blank.html";
        public static Uri GetOAuthUri() => new Uri($"https://oauth.vk.com/authorize?client_id={PrivateData.VkAppId}&display=page&scope=photos,offline&response_type=token&v=5.42&redirect_uri={GetRedirectUrl()}");
    }
}
