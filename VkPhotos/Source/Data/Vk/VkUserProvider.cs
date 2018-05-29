using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Data.Json;

namespace VkPhotos.Data.Vk
{
    public class VkUserProvider: IUserProvider
    {
        private IHttpClient Client = new WindowsStoreHttpClient();

        public async Task<IUser> GetAsync(UInt32 userId)
        {
            var requestUrl = $"https://api.vk.com/method/users.get?user_ids={userId}&fields=photo_100&v=5.42";
            var stringResponse = await Client.GetAsync(requestUrl).ConfigureAwait(false);

            var objectResponse = JsonObject.Parse(stringResponse);
            if (!objectResponse.ContainsKey("response") || (objectResponse["response"].ValueType != JsonValueType.Array))
                throw new VkRequestException(objectResponse);

            var items = new List<IUser>();
            foreach (var item in objectResponse["response"].GetArray())
                items.Add(VkUser.Create(item.GetObject()));
            return items[0];
        }
    }
}
