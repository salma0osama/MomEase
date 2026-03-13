using MomEase.core.DTOS.NotificationDto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MomEase.core.Interfaces
{
    public interface INotificationService
    {
        Task SendRealtimeNotificationAsync(int userId, string title, string body, string type, int? relatedEntityId = null);
        Task<IEnumerable<NotificationDto>> GetUserNotificationsAsync(int userId);
        Task<int> GetUnreadCountAsync(int userId);
        Task<bool> MarkAsReadAsync(int userId, int notificationId);
        Task<bool> MarkAllAsReadAsync(int userId);
        Task<bool> DeleteNotificationAsync(int userId, int notificationId);
    }
}
