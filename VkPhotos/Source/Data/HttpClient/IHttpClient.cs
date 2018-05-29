using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Storage.Streams;

namespace VkPhotos.Data
{
    public interface IHttpClient
    {
        Task<String> GetAsync(String url);
        Task<IBuffer> DownloadAsync(Uri uri);
        Task<String> PostAsync(String url);
        Task<String> PostAsync(String url, IEnumerable<KeyValuePair<String, String>> parameters);
        Task<String> PostAsync(String url, JsonObject content);
    }
}
