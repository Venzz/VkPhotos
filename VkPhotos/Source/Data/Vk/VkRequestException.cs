using System;
using System.Collections.Generic;
using Windows.Data.Json;

namespace VkPhotos.Data.Vk
{
    public class VkRequestException: Exception
    {
        public Int32 ErrorCode { get; }
        public IEnumerable<String> Parameters { get; }

        public VkRequestException(JsonObject data): base(VkRequestException.GetMessage(data))
        {
            var parameters = new List<String>();
            if (data.ContainsKey("error"))
            {
                var error = data["error"].GetObject();
                if (error.ContainsKey("error_code"))
                    ErrorCode = (Int32)error["error_code"].GetNumber();
                if (error.ContainsKey("request_params"))
                    foreach (var value in error["request_params"].GetArray())
                        parameters.Add(value.Stringify());
            }
            Parameters = parameters;
        }

        public VkRequestException(Int32? code, String message): base($"ErrorCode: {code}, {message}")
        {
            ErrorCode = code.HasValue ? code.Value : 0;
        }

        private static String GetMessage(JsonObject data)
        {
            if (data.ContainsKey("error"))
            {
                var error = data["error"].GetObject();
                if (error.ContainsKey("error_msg"))
                    return error["error_msg"].GetString();
            }
            return null;
        }
    }
}
