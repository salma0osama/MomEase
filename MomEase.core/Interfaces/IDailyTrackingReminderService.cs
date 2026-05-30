using MomEase.core.DTOS.TrackingReminderDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MomEase.core.Interfaces
{
    public interface IDailyTrackingReminderService
    {
        /// <summary>
        /// Send daily tracking reminders for all users
        /// </summary>
        Task SendDailyTrackingRemindersAsync();

        /// <summary>
        /// Get tracking status for a specific user's child
        /// </summary>
        Task<DailyTrackingReminderDto?> GetTrackingStatusAsync(int userId);
    }
}
