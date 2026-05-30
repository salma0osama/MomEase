using MomEase.core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MomEase.core.Interfaces
{
    public interface IDailyTrackingReminderRepository
    {
        Task<DailyTrackingReminder> CreateAsync(DailyTrackingReminder reminder);
        Task<DailyTrackingReminder?> GetByIdAsync(int reminderId);
        Task<List<DailyTrackingReminder>> GetByUserIdAndDateAsync(int userId, DateTime date);
        Task<List<DailyTrackingReminder>> GetUnsendRemindersAsync();
        Task<DailyTrackingReminder> UpdateAsync(DailyTrackingReminder reminder);
        Task<bool> DeleteAsync(int reminderId);
        Task<int> SaveChangesAsync();
    }
}
