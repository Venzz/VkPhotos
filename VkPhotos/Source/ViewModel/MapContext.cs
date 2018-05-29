using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VkPhotos.Data.Vk;
using VkPhotos.Model;
using Windows.Foundation;

namespace VkPhotos.ViewModel
{
    public class MapContext: PageContext
    {
        private Boolean _IsSearchButtonEnabled = true;
        private Object Sync = new Object();
        private PhotoBatchResult PhotosBatchResult;
        private Int64 ActiveProgressiveSearchId;
        private ProgressivePhotoSearch ProgressiveSearch;
        private IDictionary<UInt32, Photo> Photos = new Dictionary<UInt32, Photo>();

        public Boolean IsSearchButtonEnabled { get { return _IsSearchButtonEnabled; } private set { _IsSearchButtonEnabled = value; OnPropertyChanged(nameof(IsSearchButtonEnabled)); } }
        public User User { get; private set; }
        public Boolean IsUserAuthorized => User != null;

        public event TypedEventHandler<MapContext, IReadOnlyCollection<Photo>> PhotosAdded = delegate { };
        public event TypedEventHandler<MapContext, IReadOnlyCollection<Photo>> PhotosRemoved = delegate { };
        public event EventHandler AuthorizationFailed = delegate { };



        public Task InitializeAsync() => Task.Run(() =>
        {
            User = App.Model.Users.GetCurrentUser();
            OnPropertyChanged(nameof(User), nameof(IsUserAuthorized));
        });

        public Task UpdateStateAsync() => Task.Run(() =>
        {
            User = App.Model.Users.GetCurrentUser();
            OnPropertyChanged(nameof(User), nameof(IsUserAuthorized));
        });

        public Task LoadPersonalPhotosAsync() => Task.Run(async () =>
        {
            var progressId = ShowProgress(Strings.ProgressIndicator_GettingPhotos);
            IsSearchButtonEnabled = false;
            try
            {
                if (PhotosBatchResult != null)
                {
                    PhotosBatchResult.Cancel = true;
                    PhotosBatchResult.MoreItemsReady -= OnMoreItemsReady;
                }

                var dates = GetDates();
                PhotosBatchResult = await App.Model.Photos.GetUsersAsync(User).ConfigureAwait(false);
                PhotosBatchResult.MoreItemsReady += OnMoreItemsReady;
                var newPhotos = MergePhotos(PhotosBatchResult.Items);
                if (newPhotos.Count > 0)
                    PhotosAdded(this, newPhotos);

                HideProgress(progressId, Strings.GetProgressIndicatorSearchingPhotosResult(newPhotos.Count));
            }
            catch (VkRequestException authorizationFailed) when (authorizationFailed.ErrorCode == 5)
            {
                AuthorizationFailed(this, EventArgs.Empty);
                HideProgress(progressId, Strings.ProgressIndicator_SearchingPhotosResult_Error);
            }
            catch (Exception exception)
            {
                App.Diagnostics.Error.Log("MapContext.LoadPersonalPhotosAsync", exception);
                HideProgress(progressId, Strings.ProgressIndicator_SearchingPhotosResult_Error);
            }
            finally
            {
                IsSearchButtonEnabled = true;
            }
        });

        public Task StartProgressiveSearchAsync(MapViewSettings viewSettings, PhotoTag tag) => Task.Run(() =>
        {
            if (ProgressiveSearch != null)
                ProgressiveSearch.Cancel = true;
            
            ProgressiveSearch = App.Model.Photos.CreateProgressiveSearch(tag, viewSettings.Location, viewSettings.FromDate, viewSettings.ToDate);
            ActiveProgressiveSearchId = ProgressiveSearch.Id;
            ProgressiveSearch.ChunkSearchingStarted += OnProgressiveChunkSearchingStarted;
            ProgressiveSearch.ChunkSearchingCompleted += OnProgressiveChunkSearchingCompleted;
            ProgressiveSearch.ChunkSearchingFailed += OnProgressiveChunkSearchingFailed;
            ProgressiveSearch.SearchingFinished += OnProgressiveSearchingFinished;
            return ProgressiveSearch.PerformAsync();
        });

        private void OnProgressiveChunkSearchingStarted(Object sender, EventArgs args)
        {
            var search = (ProgressivePhotoSearch)sender;
            IsSearchButtonEnabled = false;
            search.Tag = ShowProgress(Strings.ProgressIndicator_SearchingPhotos);
        }

        private void OnProgressiveChunkSearchingCompleted(ProgressivePhotoSearch sender, IEnumerable<Photo> items)
        {
            var newPhotos = MergePhotos(items);
            if (newPhotos.Count > 0)
                PhotosAdded(this, newPhotos);

            HideProgress((ProgressNotificationId)sender.Tag, Strings.GetProgressIndicatorSearchingPhotosResult(newPhotos.Count));
            if (ActiveProgressiveSearchId == sender.Id)
                IsSearchButtonEnabled = true;
        }

        private void OnProgressiveChunkSearchingFailed(ProgressivePhotoSearch sender, Exception exception)
        {
            App.Diagnostics.Error.Log("MapContext.SearchAsync", exception);
            IsSearchButtonEnabled = true;

            HideProgress((ProgressNotificationId)sender.Tag, Strings.ProgressIndicator_SearchingPhotosResult_Error);
            if (ActiveProgressiveSearchId == sender.Id)
                IsSearchButtonEnabled = true;
        }

        private void OnProgressiveSearchingFinished(Object sender, EventArgs args)
        {
            var search = (ProgressivePhotoSearch)sender;
            if (search.Tag != null)
                HideProgress((ProgressNotificationId)search.Tag);

            search.ChunkSearchingStarted -= OnProgressiveChunkSearchingStarted;
            search.ChunkSearchingCompleted -= OnProgressiveChunkSearchingCompleted;
            search.ChunkSearchingFailed -= OnProgressiveChunkSearchingFailed;
            search.SearchingFinished -= OnProgressiveSearchingFinished;
            if (ActiveProgressiveSearchId == search.Id)
                IsSearchButtonEnabled = true;
        }

        private async void OnMoreItemsReady(PhotoBatchResult sender, PhotoBatchResult.EventArgs args)
        {
            var newPhotos = MergePhotos(args.Items);
            if (newPhotos.Count > 0)
            {
                PhotosAdded(this, newPhotos);
                await ShowProgressMessageAsync(Strings.GetProgressIndicatorSearchingPhotosResult(newPhotos.Count));
            }
        }

        public Task SignOutAsync() => Task.Run(() =>
        {
            App.Model.Users.SignOut();
            User = null;
            OnPropertyChanged(nameof(User), nameof(IsUserAuthorized));

            lock (Sync)
            {
                var removedPhotos = new List<Photo>();
                foreach (var photo in Photos)
                {
                    if (photo.Value.Tag == PhotoTag.Personal)
                        removedPhotos.Add(photo.Value);
                    else if (photo.Value.Tag.HasFlag(PhotoTag.Personal))
                        photo.Value.Tag &= ~PhotoTag.Personal;
                }
                if (removedPhotos.Count > 0)
                    PhotosRemoved(this, removedPhotos);
            }
        });

        private IReadOnlyCollection<Photo> MergePhotos(IEnumerable<Photo> photos)
        {
            var newPhotos = new List<Photo>();
            foreach (var photo in photos)
            {
                if (!Photos.ContainsKey(photo.Id))
                {
                    Photos.Add(photo.Id, photo);
                    newPhotos.Add(photo);
                }
                else
                {
                    Photos[photo.Id].Tag |= photo.Tag;
                }
            }
            return newPhotos;
        }

        private Tuple<DateTime, DateTime> GetDates()
        {
            if (App.Settings.IsPastPeriodEnabled)
            {
                return App.Settings.PastPeriod.GetDates();
            }
            else if (App.Settings.IsPeriodEnabled)
            {
                return new Tuple<DateTime, DateTime>(App.Settings.FromDate, App.Settings.ToDate);
            }
            else
            {
                throw new InvalidOperationException();
            }
        }
    }
}
