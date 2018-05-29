using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VkPhotos.Data;
using VkPhotos.Model.Map;
using Windows.Foundation;

namespace VkPhotos.Model
{
    public class ProgressivePhotoSearch
    {
        private IPhotoProvider Provider;
        private PhotoTag PhotoTag;
        private GeoPoint Location;
        private DateTime StartDate;
        private DateTime EndDate;
        private UInt16 Radius;

        public Int64 Id { get; } = DateTime.UtcNow.Ticks;
        public Boolean Cancel { get; set; }
        public Object Tag { get; set; }

        public event EventHandler ChunkSearchingStarted = delegate { };
        public event TypedEventHandler<ProgressivePhotoSearch, IEnumerable<Photo>> ChunkSearchingCompleted = delegate { };
        public event TypedEventHandler<ProgressivePhotoSearch, Exception> ChunkSearchingFailed = delegate { };
        public event EventHandler SearchingFinished = delegate { };



        public ProgressivePhotoSearch(IPhotoProvider provider, PhotoTag photoTag, GeoPoint location, DateTime startDate, DateTime endDate, UInt16 radius)
        {
            Provider = provider;
            PhotoTag = photoTag;
            Location = location;
            StartDate = startDate;
            EndDate = endDate;
            Radius = radius;
        }

        public async Task PerformAsync()
        {
            var endDate = EndDate;
            while (!Cancel && endDate > StartDate)
            {
                var chunk = new Chunk() { StartDate = StartDate, EndDate = endDate };
                try
                {
                    ChunkSearchingStarted(this, EventArgs.Empty);
                    var searchResult = await Provider.SearchAsync(Location.Latitude, Location.Longitude, StartDate, endDate, 0, 1000, Radius).ConfigureAwait(false);
                    endDate = ((searchResult.Total > 1000) && searchResult.StartDate.HasValue) ? searchResult.StartDate.Value.AddMinutes(-1) : StartDate;

                    var photos = new List<Photo>();
                    foreach (var photoMetadata in searchResult.Photos)
                    {
                        var photo = Photo.Create(photoMetadata);
                        if (photo != null)
                        {
                            photos.Add(photo);
                            photo.Tag = PhotoTag;
                        }
                    }
                    ChunkSearchingCompleted(this, photos);
                }
                catch (Exception exception)
                {
                    ChunkSearchingFailed(this, exception);
                }
            }
            SearchingFinished(this, EventArgs.Empty);
        }

        private class Chunk
        {
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
            public Boolean IsComplete { get; set; }
        }
    }
}
