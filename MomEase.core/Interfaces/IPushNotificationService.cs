using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MomEase.core.Interfaces
{
    public interface IPushNotificationService
    {
        Task SendPushNotificationAsync(
            int userId,
            string title,
            string message,
            string type,
            int? relatedId);
    }
}
