using System;

namespace VkPhotos
{
    [Flags]
    public enum PhotoTag
    {
        None = 0,
        Public = 1,
        Personal = 2,
        SharedLink = 4,
    }
}
