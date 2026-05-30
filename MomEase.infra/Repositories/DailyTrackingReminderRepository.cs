using Microsoft.EntityFrameworkCore;
using MomEase.core.Entities;
using MomEase.core.Interfaces;
using MomEase.infra.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MomEase.infra.Repositories
{
    public class DailyTrackingReminderRepository : IDailyTrackingReminderRepository
    {
        private readonly MomEaseDbContext _context;

        public DailyTrackingReminderRepository(MomEaseDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<DailyTrackingReminder> CreateAsync(DailyTrackingReminder reminder)
        {
            if (reminder == null)
                throw new ArgumentNullException(nameof(reminder));

            await _context.DailyTrackingReminders.AddAsync(reminder);
            await _context.SaveChangesAsync();
            return reminder;
        }

        public async Task<DailyTrackingReminder?> GetByIdAsync(int reminderId)
        {
            return await _context.DailyTrackingReminders
                .Include(r => r.User)
                .Include(r => r.Child)
                .FirstOrDefaultAsync(r => r.ReminderId == reminderId);
        }

        public async Task<List<DailyTrackingReminder>> GetByUserIdAndDateAsync(int userId, DateTime date)
        {
            var dateOnly = date.Date;
            return await _context.DailyTrackingReminders
                .Include(r => r.User)
                .Include(r => r.Child)
                .Where(r => r.UserId == userId && r.ReminderDate.Date == dateOnly)
                .ToListAsync();
        }

        public async Task<List<DailyTrackingReminder>> GetUnsendRemindersAsync()
        {
            return await _context.DailyTrackingReminders
                .Include(r => r.User)
                .Include(r => r.Child)
                .Where(r => !r.IsSent && r.ReminderDate.Date == DateTime.Now.Date)
                .ToListAsync();
        }

        public async Task<DailyTrackingReminder> UpdateAsync(DailyTrackingReminder reminder)
        {
            if (reminder == null)
                throw new ArgumentNullException(nameof(reminder));

            _context.DailyTrackingReminders.Update(reminder);
            await _context.SaveChangesAsync();
            return reminder;
        }

        public async Task<bool> DeleteAsync(int reminderId)
        {
            var reminder = await GetByIdAsync(reminderId);
            if (reminder == null)
                return false;

            _context.DailyTrackingReminders.Remove(reminder);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}
