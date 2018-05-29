using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Venz.Data;
using VkPhotos.Model;

namespace VkPhotos.ViewModel
{
    public class SettingsContext: ObservableObject
    {
        public Boolean AutomaticSearch { get { return App.Settings.AutomaticSearch; } set { App.Settings.AutomaticSearch = value; } }
        public Boolean IsPastPeriodEnabled => App.Settings.IsPastPeriodEnabled;
        public Boolean IsPeriodEnabled => App.Settings.IsPeriodEnabled;
        public PastPeriodItem SelectedPastPeriod => new PastPeriodItem(App.Settings.PastPeriod);
        public IEnumerable<PastPeriodItem> PastPeriods { get; }
        public DateTimeOffset FromDate { get { return App.Settings.FromDate.ToLocalTime(); } set { App.Settings.FromDate = value.DateTime.ToUniversalTime(); } }
        public DateTimeOffset ToDate { get { return App.Settings.ToDate.ToLocalTime(); } set { App.Settings.ToDate = value.DateTime.ToUniversalTime(); } }
        public Boolean IsCachingEnabled { get { return App.Settings.IsCachingEnabled; } set { App.Settings.IsCachingEnabled = value; OnIsCachingEnabledChanged(); } }
        public LocalFileFolderSize LocalFileFolderSize { get; private set; }
        public Boolean IsLocalFilesOperationInProgress { get; private set; } = true;
        public Boolean IsClearCacheButtonEnabled => !IsLocalFilesOperationInProgress && (LocalFileFolderSize != null) && !LocalFileFolderSize.IsEmpty && IsCachingEnabled;



        public SettingsContext()
        {
            PastPeriods = new List<PastPeriodItem>()
            {
                new PastPeriodItem(PastPeriod.Hour),
                new PastPeriodItem(PastPeriod.Day),
                new PastPeriodItem(PastPeriod.Month),
                new PastPeriodItem(PastPeriod.Year)
            };
        }

        public Task InitializeAsync() => Task.Run(async () =>
        {
            LocalFileFolderSize = await App.Model.LocalFiles.GetFolderSizeAsync().ConfigureAwait(false);
            IsLocalFilesOperationInProgress = false;
            OnPropertyChanged(nameof(LocalFileFolderSize), nameof(IsLocalFilesOperationInProgress), nameof(IsClearCacheButtonEnabled));
        });

        public void EnablePastPeriod()
        {
            App.Settings.IsPastPeriodEnabled = true;
            App.Settings.IsPeriodEnabled = false;
            OnPropertyChanged(nameof(IsPastPeriodEnabled), nameof(IsPeriodEnabled));
        }

        public void EnablePeriod()
        {
            App.Settings.IsPastPeriodEnabled = false;
            App.Settings.IsPeriodEnabled = true;
            OnPropertyChanged(nameof(IsPastPeriodEnabled), nameof(IsPeriodEnabled));
        }

        public void DisablePeriod() => EnablePastPeriod();

        public void DisablePastPeriod() => EnablePeriod();

        public void SetPastPeriod(Object item) => App.Settings.PastPeriod = ((PastPeriodItem)item).Value;

        private void OnIsCachingEnabledChanged() => OnPropertyChanged(nameof(IsCachingEnabled), nameof(IsClearCacheButtonEnabled));

        public Task ClearCacheAsync()
        {
            return Task.Run(async () =>
            {
                IsLocalFilesOperationInProgress = true;
                OnPropertyChanged(nameof(IsLocalFilesOperationInProgress), nameof(IsClearCacheButtonEnabled));
                try
                {
                    await App.Model.LocalFiles.ClearAsync().ConfigureAwait(false);
                    LocalFileFolderSize = new LocalFileFolderSize(0, 0);
                }
                catch (Exception exception)
                {
                    App.Diagnostics.Telemetry.Log("ClearCache", exception);
                }
                IsLocalFilesOperationInProgress = false;
                OnPropertyChanged(nameof(LocalFileFolderSize), nameof(IsLocalFilesOperationInProgress), nameof(IsClearCacheButtonEnabled));
            });
        }



        public struct PastPeriodItem
        {
            public PastPeriod Value { get; }
            public PastPeriodItem(PastPeriod value) { Value = value; }
            public override String ToString() => Strings.GetTitle(Value);
        }
    }
}
