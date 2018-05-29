using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace VkPhotos.Data
{
    public interface IUserProvider
    {
        Task<IUser> GetAsync(UInt32 userId);
    }
}
