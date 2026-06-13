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
    public class ChildRepository : IChildRepository
    {
        private readonly MomEaseDbContext _context;

        public ChildRepository(MomEaseDbContext context)
        {
            _context = context;
        }

        // POST /api/children - إضافة طفل جديد
        public async Task<Child> AddChildAsync(Child child)
        {
            await _context.Children.AddAsync(child);
            await _context.SaveChangesAsync();
            return child;
        }

        // GET /api/children - الحصول على جميع أطفال المستخدم
        public async Task<List<Child>> GetUserChildrenAsync(int userId)
        {
            return await _context.Children
                .Where(c => c.UserId == userId)
                .OrderByDescending(c => c.BirthDate)
                .ToListAsync();
        }

        // GET /api/children/{id} - الحصول على طفل معين
        public async Task<Child> GetChildByIdAsync(int childId)
        {
            return await _context.Children
                .FirstOrDefaultAsync(c => c.ChildId == childId);
        }
        public async Task<List<Child>> GetByUserIdAsync(int userId)
        {
            return await _context.Children
                .Where(c => c.UserId == userId)
                .ToListAsync();
        }
        // PUT /api/children/{id} - تحديث بيانات طفل
        public async Task<Child> UpdateChildAsync(Child child)
        {
            _context.Children.Update(child);
            await _context.SaveChangesAsync();
            return child;
        }

        // DELETE /api/children/{id} - حذف طفل
        public async Task<bool> DeleteChildAsync(Child child)
        {
            // 1. GrowthRecords
            var growthRecords = await _context.GrowthRecords
                .Where(g => g.ChildId == child.ChildId).ToListAsync();
            _context.GrowthRecords.RemoveRange(growthRecords);

            // 2. GrowthReports
            var growthReports = await _context.GrowthReports
                .Where(g => g.ChildId == child.ChildId).ToListAsync();
            _context.GrowthReports.RemoveRange(growthReports);

            // 3. ChildSleepRecords
            var sleepRecords = await _context.ChildSleepRecords
                .Where(s => s.ChildId == child.ChildId).ToListAsync();
            _context.ChildSleepRecords.RemoveRange(sleepRecords);

            // 4. ChildFeedingRecords
            var feedingRecords = await _context.ChildFeedingRecords
                .Where(f => f.ChildId == child.ChildId).ToListAsync();
            _context.ChildFeedingRecords.RemoveRange(feedingRecords);

            // 5. SkinAnalyses (Restrict)
            var skinAnalyses = await _context.SkinAnalyses
                .Where(s => s.ChildId == child.ChildId).ToListAsync();
            _context.SkinAnalyses.RemoveRange(skinAnalyses);

            // 6. CryAnalyses (Restrict)
            var cryAnalyses = await _context.CryAnalyses
                .Where(c => c.ChildId == child.ChildId).ToListAsync();
            _context.CryAnalyses.RemoveRange(cryAnalyses);

            // 7. DailyTrackingReminders (Restrict)
            var reminders = await _context.DailyTrackingReminders
                .Where(d => d.ChildId == child.ChildId).ToListAsync();
            _context.DailyTrackingReminders.RemoveRange(reminders);

            // ChildVaccinations بتتمسح Cascade تلقائي
            _context.Children.Remove(child);
            await _context.SaveChangesAsync();
            return true;
        }
        // POST /api/children/{id}/photo - تحديث صورة الطفل
        public async Task<Child> UpdateChildPhotoAsync(int childId, string photoUrl)
        {
            var child = await _context.Children.FindAsync(childId);
            if (child != null)
            {
                child.PhotoUrl = photoUrl;
                await _context.SaveChangesAsync();
            }
            return child;
        }

        // DELETE /api/children/{id}/photo - حذف صورة الطفل
        public async Task<Child> DeleteChildPhotoAsync(int childId)
        {
            var child = await _context.Children.FindAsync(childId);
            if (child != null)
            {
                child.PhotoUrl = null;
                await _context.SaveChangesAsync();
            }
            return child;
        }

        // Helper: التحقق من ملكية الطفل للمستخدم
        public async Task<bool> IsChildOwnedByUserAsync(int childId, int userId)
        {
            return await _context.Children
                .AnyAsync(c => c.ChildId == childId && c.UserId == userId);
        }

        // Helper: عدد الأطفال للمستخدم
        public async Task<int> GetUserChildrenCountAsync(int userId)
        {
            return await _context.Children
                .CountAsync(c => c.UserId == userId);
        }
    }
}
