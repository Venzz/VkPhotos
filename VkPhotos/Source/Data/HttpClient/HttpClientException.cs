using System;

namespace VkPhotos.Data
{
    public class HttpClientException: Exception
    {
        public String RawResponse { get; }

        public HttpClientException(): base("No internet connection.") { }

        public HttpClientException(Int32 statusCode, String responseContent): base($"Request failed with response code {statusCode}.")
        {
            System.Diagnostics.Debug.WriteLine($"{statusCode}\n{responseContent}");
            RawResponse = responseContent;
        }
    }
}
