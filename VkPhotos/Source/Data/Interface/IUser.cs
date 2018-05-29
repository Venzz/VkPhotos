using System;

namespace VkPhotos.Data
{
    public interface IUser
    {
        UInt32 Id { get; }
        String FirstName { get; }
        String LastName { get; }
        Uri Photo { get; }
    }
}
