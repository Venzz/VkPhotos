using System;
using VkPhotos.View.Converters;

namespace VkPhotos.Model
{
    public class LocalFileFolderSize
    {
        public UInt64 Size { get; private set; }
        public UInt32 Files { get; private set; }
        public Boolean IsEmpty => (Size == 0) && (Files == 0);

        public LocalFileFolderSize(UInt64 size, UInt32 files)
        {
            Size = size;
            Files = files;
        }

        public void AddFile(UInt64 size)
        {
            Size += size;
            Files++;
        }

        public void Clear()
        {
            Size = 0;
            Files = 0;
        }

        public override String ToString() => (Files > 0) ? String.Format(Strings.Text_LocalFileFolderSize, Files, UIntToBytesSize.Get(Size)) : Strings.Text_LocalFileFolderSize_Empty;
    }
}
