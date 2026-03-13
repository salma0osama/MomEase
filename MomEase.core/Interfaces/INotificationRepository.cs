using MomEase.core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MomEase.core.Interfaces
{
    public interface INotificationRepository
    {
        Task<Notifications> CreateAsync(Notifications notification);
        Task<IEnumerable<Notifications>> GetByUserIdAsync(int userId);
        Task<Notifications?> GetByIdAsync(int userId, int notificationId);
        Task<int> GetUnreadCountAsync(int userId);
        Task<Notifications> UpdateAsync(Notifications notification);
        Task<bool> MarkAllAsReadAsync(int userId);
        Task<bool> DeleteAsync(int userId, int notificationId);
    }
}
