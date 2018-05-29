using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;

namespace VkPhotos.Model
{
    public class LocalFileCollection
    {
        private Lazy<Task<StorageFolder>> Folder;
        private Lazy<Task<LocalFileFolderSize>> FolderSize;
        private IDictionary<UInt32, StorageFile> Files = new Dictionary<UInt32, StorageFile>();

        public LocalFileCollection()
        {
            Folder = new Lazy<Task<StorageFolder>>(() => ApplicationData.Current.LocalFolder.CreateFolderAsync("Cache", CreationCollisionOption.OpenIfExists).AsTask(), isThreadSafe: true);
            FolderSize = new Lazy<Task<LocalFileFolderSize>>(InitializeFolderSizeAsync, isThreadSafe: true);
        }

        public Task<LocalFileFolderSize> GetFolderSizeAsync() => FolderSize.Value;

        public StorageFile TryGet(UInt32 id)
        {
            var file = (StorageFile)null;
            Files.TryGetValue(id, out file);
            return file;
        }

        public async Task<Uri> StoreAsync(UInt32 id, IBuffer content)
        {
            var folder = await Folder.Value.ConfigureAwait(false);
            var file = await folder.CreateFileAsync($"{id}.jpg", CreationCollisionOption.OpenIfExists).AsTask().ConfigureAwait(false);
            if (!Files.ContainsKey(id))
                Files.Add(id, file);
            await FileIO.WriteBufferAsync(file, content).AsTask().ConfigureAwait(false);
            if (FolderSize.IsValueCreated)
                (await FolderSize.Value.ConfigureAwait(false)).AddFile(content.Length);
            return new Uri($"ms-appdata:///local/Cache/{id}.jpg", UriKind.Absolute);
        }

        public async Task ClearAsync()
        {
            var folder = await Folder.Value.ConfigureAwait(false);
            if (FolderSize.IsValueCreated)
                (await FolderSize.Value.ConfigureAwait(false)).Clear();
            Files.Clear();
            await folder.DeleteAsync();
        }

        private async Task<LocalFileFolderSize> InitializeFolderSizeAsync()
        {
            var folder = await Folder.Value.ConfigureAwait(false);
            var size = 0UL;
            var amount = 0U;
            foreach (var file in await folder.GetFilesAsync().AsTask().ConfigureAwait(false))
            {
                size += (await file.GetBasicPropertiesAsync().AsTask().ConfigureAwait(false)).Size;
                amount++;
                var id = Convert.ToUInt32(file.DisplayName);
                if (!Files.ContainsKey(id))
                    Files.Add(id, file);
            }
            return new LocalFileFolderSize(size, amount);
        }
    }
}
