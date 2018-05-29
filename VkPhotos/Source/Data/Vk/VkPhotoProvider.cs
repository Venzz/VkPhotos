using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using Venz.Extensions;
using Windows.Data.Json;

namespace VkPhotos.Data.Vk
{
    public class VkPhotoProvider: IPhotoProvider
    {
        private IHttpClient Client = new WindowsStoreHttpClient();

        public async Task<PhotosSearchResult> SearchAsync(Double latitude, Double longitude, DateTime startTime, DateTime endTime, UInt32 offset, UInt32 count, UInt32 radius)
        {
            var requestUrl = $"https://api.vk.com/method/photos.search?lat={latitude.ToString(CultureInfo.InvariantCulture)}&long={longitude.ToString(CultureInfo.InvariantCulture)}&start_time={startTime.ToUnixTimestamp()}&end_time={endTime.ToUnixTimestamp()}&offset={offset}&count={count}&radius={radius}&access_token={PrivateData.VkAppServiceToken}&v=5.42&photo_sizes=1";
            var stringResponse = await Client.GetAsync(requestUrl).ConfigureAwait(false);
            return ParseUsingNewton(stringResponse);
        }

        public async Task<PhotosSearchResult> GetAllAsync(UInt32 owner, String accessToken, UInt32 offset, UInt32 count)
        {
            var requestUrl = $"https://api.vk.com/method/execute.getUserGeoPhotos?owner_id={owner}&offset={offset}&count={count}&access_token={accessToken}&v=5.42";
            var stringResponse = await Client.GetAsync(requestUrl).ConfigureAwait(false);
            return ParseUsingNewton(stringResponse); 
        }

        private static PhotosSearchResult ParseUsingNewton(String stringResponse)
        {
            using (var stringReader = new StringReader(stringResponse))
            using (var reader = new JsonTextReader(stringReader))
            {
                var gotResponse = false;
                var count = (UInt32?)null;
                var gotItems = false;
                var readItems = false;
                var items = (IReadOnlyCollection<IPhotoMetadata>)new List<IPhotoMetadata>();
                var preciseStartDate = (DateTime?)null;
                var preciseEndDate = (DateTime?)null;
                while (reader.Read())
                {
                    if (!gotResponse)
                    {
                        gotResponse = ((reader.TokenType == JsonToken.PropertyName) && "response".Equals(reader.Value));
                        if ((reader.TokenType == JsonToken.PropertyName) && "error".Equals(reader.Value))
                        {
                            var errorCode = (Int32?)null;
                            var errorMessage = (String)null;
                            while (reader.Read() && (reader.TokenType != JsonToken.EndObject))
                            {
                                if (reader.TokenType == JsonToken.PropertyName)
                                {
                                    if ("error_code".Equals(reader.Value))
                                    {
                                        reader.Read();
                                        errorCode = Convert.ToInt32(reader.Value);
                                    }
                                    if ("error_msg".Equals(reader.Value))
                                    {
                                        reader.Read();
                                        errorMessage = Convert.ToString(reader.Value);
                                    }
                                    if ("requrest_params".Equals(reader.Value))
                                    {
                                        while (reader.Read() && (reader.TokenType != JsonToken.EndArray)) ;
                                    }
                                }
                            }
                            throw new VkRequestException(errorCode, errorMessage);
                        }
                    }
                    else if (!count.HasValue)
                    {
                        if ((reader.TokenType == JsonToken.PropertyName) && "count".Equals(reader.Value))
                        {
                            reader.Read();
                            count = Convert.ToUInt32(reader.Value);
                        }
                    }
                    else if (!gotItems)
                    {
                        gotItems = ((reader.TokenType == JsonToken.PropertyName) && "items".Equals(reader.Value));
                    }
                    else if (!readItems)
                    {
                        var result = ParseItemsUsingNewton(reader);
                        items = result.Item1;
                        preciseStartDate = result.Item2;
                        preciseEndDate = result.Item3;
                        readItems = true;
                    }
                }
                return new PhotosSearchResult(count.HasValue ? count.Value : 0, preciseStartDate, preciseEndDate, items);
            }
        }

        private static PhotosSearchResult ParseUsingWindows(String stringResponse)
        {
            var objectResponse = JsonObject.Parse(stringResponse);
            if (!objectResponse.ContainsKey("response"))
                throw new VkRequestException(objectResponse);

            var response = objectResponse["response"].GetObject();
            var result = new List<IPhotoMetadata>();
            if (!response.ContainsKey("items") || (response["items"].ValueType != JsonValueType.Array) || !response.ContainsKey("count"))
                return new PhotosSearchResult(0, result);

            var itemsStartDate = (DateTime?)null;
            var itemsEndDate = (DateTime?)null;
            foreach (var item in response["items"].GetArray())
            {
                var photoMetadata = VkPhotoMetadata.Create(item.GetObject());
                if (!itemsEndDate.HasValue)
                    itemsEndDate = photoMetadata.Date;
                if (photoMetadata.Date.HasValue)
                    itemsStartDate = photoMetadata.Date;
                result.Add(photoMetadata);
            }
            return new PhotosSearchResult((UInt32)response["count"].GetNumber(), itemsStartDate, itemsEndDate, result);
        }

        private static Tuple<IReadOnlyCollection<IPhotoMetadata>, DateTime?, DateTime?> ParseItemsUsingNewton(JsonTextReader reader)
        {
            var items = new List<IPhotoMetadata>();

            while (reader.TokenType != JsonToken.StartArray)
                reader.Read();

            var startDate = (DateTime?)null;
            var endDate = (DateTime?)null;
            while (reader.Read() && (reader.TokenType != JsonToken.EndArray))
            {
                while (reader.TokenType != JsonToken.StartObject)
                    reader.Read();

                var id = (UInt32?)null;
                var owner = (OwnerId)null;
                var lat = (Double?)null;
                var lon = (Double?)null;
                var date = (DateTime?)null;
                var text = (String)null;
                var photos = new List<IPhoto>();
                while (reader.Read() && (reader.TokenType != JsonToken.EndObject))
                {
                    if (reader.TokenType == JsonToken.PropertyName)
                    {
                        if ("id".Equals(reader.Value))
                        {
                            reader.Read();
                            id = Convert.ToUInt32(reader.Value);
                        }
                        else if ("owner_id".Equals(reader.Value))
                        {
                            reader.Read();
                            owner = OwnerId.Create(Convert.ToInt32(reader.Value));
                        }
                        else if ("lat".Equals(reader.Value))
                        {
                            reader.Read();
                            lat = Convert.ToDouble(reader.Value);
                        }
                        else if ("long".Equals(reader.Value))
                        {
                            reader.Read();
                            lon = Convert.ToDouble(reader.Value);
                        }
                        else if ("date".Equals(reader.Value))
                        {
                            reader.Read();
                            date = DateTimeExtensions.FromUnixTimestamp(Convert.ToInt32(reader.Value));
                            if (!endDate.HasValue)
                                endDate = date;
                            startDate = date;
                        }
                        else if ("text".Equals(reader.Value))
                        {
                            reader.Read();
                            text = Convert.ToString(reader.Value);
                        }
                        else if ("sizes".Equals(reader.Value))
                        {
                            while (reader.Read() && (reader.TokenType != JsonToken.EndArray))
                            {
                                var src = (String)null;
                                var width = (UInt32?)null;
                                var height = (UInt32?)null;
                                var type = (String)null;
                                while (reader.Read() && (reader.TokenType != JsonToken.EndObject))
                                {
                                    if (reader.TokenType == JsonToken.PropertyName)
                                    {
                                        if ("src".Equals(reader.Value))
                                        {
                                            reader.Read();
                                            src = Convert.ToString(reader.Value);
                                        }
                                        else if ("width".Equals(reader.Value))
                                        {
                                            reader.Read();
                                            width = Convert.ToUInt32(reader.Value);
                                        }
                                        else if ("height".Equals(reader.Value))
                                        {
                                            reader.Read();
                                            height = Convert.ToUInt32(reader.Value);
                                        }
                                        else if ("type".Equals(reader.Value))
                                        {
                                            reader.Read();
                                            type = Convert.ToString(reader.Value);
                                        }
                                    }
                                }
                                if ((src != null) && width.HasValue && (width.Value > 0) && height.HasValue && (height.Value > 0) && (type != null))
                                    photos.Add(new VkPhoto(new Windows.Foundation.Size(width.Value, height.Value), new Uri(src), type));
                            }
                        }
                    }
                }
                items.Add(new VkPhotoMetadata(id, owner, lat, lon, date, text, photos));
            }
            return new Tuple<IReadOnlyCollection<IPhotoMetadata>, DateTime?, DateTime?>(items, startDate, endDate);
        }
    }
}
