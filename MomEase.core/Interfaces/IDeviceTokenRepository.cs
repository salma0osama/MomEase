using MomEase.core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MomEase.core.Repositories
{
    public interface IDeviceTokenRepository
    {
        Task<DeviceToken> AddOrUpdateTokenAsync(int userId, string token);
        Task<bool> RemoveTokenAsync(string token);
        Task<bool> RemoveAllUserTokensAsync(int userId);
    }
}
