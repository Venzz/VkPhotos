using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.Web.Http;

namespace VkPhotos.Data
{
    public class WindowsStoreHttpClient: IHttpClient
    {
        private HttpClient Client;

        public WindowsStoreHttpClient()
        {
            Client = new HttpClient();
        }

        public async Task<String> GetAsync(String url)
        {
            try
            {
                var response = await Client.GetAsync(new Uri(url, UriKind.Absolute)).AsTask().ConfigureAwait(false);
                var content = await response.Content.ReadAsStringAsync().AsTask().ConfigureAwait(false);
                if (!response.IsSuccessStatusCode)
                    throw new HttpClientException((Int32)response.StatusCode, content);
                return content;
            }
            catch (Exception error)
            {
                if (error.HResult == unchecked((Int32)0x80072EE7))
                    throw new HttpClientException();
                throw;
            }
        }

        public async Task<IBuffer> DownloadAsync(Uri uri)
        {
            try
            {
                var response = await Client.GetAsync(uri).AsTask().ConfigureAwait(false);
                var content = await response.Content.ReadAsBufferAsync().AsTask().ConfigureAwait(false);
                if (!response.IsSuccessStatusCode)
                    throw new HttpClientException((Int32)response.StatusCode, "");
                return content;
            }
            catch (Exception error)
            {
                if (error.HResult == unchecked((Int32)0x80072EE7))
                    throw new HttpClientException();
                throw;
            }
        }

        public async Task<String> PostAsync(String url)
        {
            try
            {
                var response = await Client.PostAsync(new Uri(url, UriKind.Absolute), null).AsTask().ConfigureAwait(false);
                var content = await response.Content.ReadAsStringAsync().AsTask().ConfigureAwait(false);
                if (!response.IsSuccessStatusCode)
                    throw new HttpClientException((Int32)response.StatusCode, content);
                return content;
            }
            catch (Exception error)
            {
                if (error.HResult == unchecked((Int32)0x80072EE7))
                    throw new HttpClientException();
                throw;
            }
        }

        public async Task<String> PostAsync(String url, IEnumerable<KeyValuePair<String, String>> parameters)
        {
            try
            {
                var response = await Client.PostAsync(new Uri(url, UriKind.Absolute), new HttpFormUrlEncodedContent(parameters)).AsTask().ConfigureAwait(false);
                var content = await response.Content.ReadAsStringAsync().AsTask().ConfigureAwait(false);
                if (!response.IsSuccessStatusCode)
                    throw new HttpClientException((Int32)response.StatusCode, content);
                return content;
            }
            catch (Exception error)
            {
                if (error.HResult == unchecked((Int32)0x80072EE7))
                    throw new HttpClientException();
                throw;
            }
        }

        public async Task<String> PostAsync(String url, JsonObject content)
        {
            try
            {
                var response = await Client.PostAsync(new Uri(url, UriKind.Absolute), new HttpStringContent(content.Stringify())).AsTask().ConfigureAwait(false);
                var responseContent = await response.Content.ReadAsStringAsync().AsTask().ConfigureAwait(false);
                if (!response.IsSuccessStatusCode)
                    throw new HttpClientException((Int32)response.StatusCode, responseContent);
                return responseContent;
            }
            catch (Exception error)
            {
                if (error.HResult == unchecked((Int32)0x80072EE7))
                    throw new HttpClientException();
                throw;
            }
        }

        public static async Task<String> PostAsync(String url, IEnumerable<KeyValuePair<String, String>> parameters, IEnumerable<EntitledStorageFile> content)
        {
            try
            {
                var multipartFormDataContent = new HttpMultipartFormDataContent("----Asrf456BGe4h");
                foreach (var parameter in parameters)
                    multipartFormDataContent.Add(new HttpStringContent(parameter.Value), parameter.Key);

                foreach (var contentFile in content)
                {
                    var stream = await contentFile.File.OpenAsync(FileAccessMode.Read).AsTask().ConfigureAwait(false);
                    multipartFormDataContent.Add(new HttpStreamContent(stream), contentFile.Title, contentFile.File.Name);
                }
                var response = await new HttpClient().PostAsync(new Uri(url, UriKind.Absolute), multipartFormDataContent).AsTask().ConfigureAwait(false);
                var responseContent = await response.Content.ReadAsStringAsync().AsTask().ConfigureAwait(false);
                if (!response.IsSuccessStatusCode)
                    throw new HttpClientException((Int32)response.StatusCode, responseContent);
                return responseContent;
            }
            catch (Exception error)
            {
                if (error.HResult == unchecked((Int32)0x80072EE7))
                    throw new HttpClientException();
                throw;
            }
        }

        public class EntitledStorageFile
        {
            public String Title { get; private set; }
            public StorageFile File { get; private set; }

            public EntitledStorageFile(String title, StorageFile file)
            {
                Title = title;
                File = file;
            }
        }
    }
}
