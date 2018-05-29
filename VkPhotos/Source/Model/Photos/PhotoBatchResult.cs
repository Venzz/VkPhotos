using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VkPhotos.Data;
using Windows.Foundation;

namespace VkPhotos.Model
{
    public class PhotoBatchResult
    {
        private UInt32 ReadyElements;
        private UInt16 BatchSize;
        private Func<UInt32, UInt32, Task<PhotosSearchResult>> Action;

        public IEnumerable<Photo> Items { get; }
        public Boolean Cancel { get; set; }
        public event TypedEventHandler<PhotoBatchResult, EventArgs> MoreItemsReady;



        public PhotoBatchResult(IReadOnlyCollection<Photo> items, PhotoTag tag, Func<UInt32, UInt32, Task<PhotosSearchResult>> action, UInt16 batchSize)
        {
            Items = items;
            ReadyElements = batchSize;
            Action = action;
            BatchSize = batchSize;
            StartGettingResultAsync(tag);
        }

        private async void StartGettingResultAsync(PhotoTag tag)
        {
            try
            {
                while (!Cancel)
                {
                    var searchResult = await Action.Invoke(ReadyElements, BatchSize).ConfigureAwait(false);
                    if (Cancel)
                        break;
                    ReadyElements += BatchSize;

                    var photos = new List<Photo>();
                    foreach (var photoMetadata in searchResult.Photos)
                    {
                        var photo = Photo.Create(photoMetadata);
                        if (photo != null)
                        {
                            photos.Add(photo);
                            photo.Tag = tag;
                        }
                    }
                    if ((MoreItemsReady == null) || (photos.Count == 0))
                        break;

                    var eventArgs = new EventArgs(photos);
                    MoreItemsReady(this, eventArgs);
                    if (eventArgs.Cancel)
                        break;
                }
            }
            catch (Exception)
            {

            }
        }

        public class EventArgs
        {
            public IEnumerable<Photo> Items { get; }
            public Boolean Cancel { get; set; }
            public EventArgs(IEnumerable<Photo> items) { Items = items; }
        }
    }
}
