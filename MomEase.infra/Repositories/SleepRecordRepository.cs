using Microsoft.EntityFrameworkCore;
using MomEase.core.Entities;
using MomEase.core.Interfaces;
using MomEase.infra.Data;

namespace MomEase.infra.Repositories
{
    public class SleepRecordRepository : ISleepRecordRepository
    {
        private readonly MomEaseDbContext _context;

        public SleepRecordRepository(MomEaseDbContext context)
        {
            _context = context;
        }

        public async Task<ChildSleepRecord> AddSleepRecordAsync(ChildSleepRecord record)
        {
            await _context.ChildSleepRecords.AddAsync(record);
            await _context.SaveChangesAsync();
            return record;
        }

        public async Task<List<ChildSleepRecord>> GetChildSleepRecordsAsync(int childId)
        {
            return await _context.ChildSleepRecords
                .Include(s => s.Child)
                .Include(s => s.SleepReference)
                .Where(s => s.ChildId == childId)
                .OrderByDescending(s => s.SleepDate)
                .ToListAsync();
        }

        public async Task<ChildSleepRecord> GetSleepRecordByIdAsync(int recordId)
        {
            return await _context.ChildSleepRecords
                .Include(s => s.Child)
                .Include(s => s.SleepReference)
                .FirstOrDefaultAsync(s => s.RecordId == recordId);
        }

        public async Task<ChildSleepRecord> UpdateSleepRecordAsync(ChildSleepRecord record)
        {
            _context.ChildSleepRecords.Update(record);
            await _context.SaveChangesAsync();
            return record;
        }

        public async Task<bool> DeleteSleepRecordAsync(ChildSleepRecord record)
        {
            _context.ChildSleepRecords.Remove(record);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<ChildSleepRecord>> GetSleepRecordsByDateRangeAsync(int childId, DateTime startDate, DateTime endDate)
        {
            return await _context.ChildSleepRecords
                .Include(s => s.Child)
                .Include(s => s.SleepReference)
                .Where(
                s => s.ChildId == childId &&
                s.SleepDate.Date >= startDate.Date &&
                s.SleepDate.Date <= endDate.Date)
                .OrderBy(s => s.SleepDate)
                .ToListAsync();
        }

        public async Task<bool> IsSleepRecordOwnedByUserAsync(int recordId, int userId)
        {
            return await _context.ChildSleepRecords
                .AnyAsync(s => s.RecordId == recordId && s.Child.UserId == userId);
        }

        // ✅ إضافة: منع السجلات المكررة في نفس اليوم
        public async Task<bool> ExistsForDateAsync(int childId, DateTime date, int? excludeRecordId = null)
        {
            var dateOnly = date.Date;
            var query = _context.ChildSleepRecords
                .Where(s => s.ChildId == childId && s.SleepDate.Date == dateOnly);

            if (excludeRecordId.HasValue)
            {
                query = query.Where(s => s.RecordId != excludeRecordId.Value);
            }

            return await query.AnyAsync();
        }

        // ✅ إضافة: Get Last N Days (كان مفقود)
        public async Task<List<ChildSleepRecord>> GetLastNDaysAsync(int childId, int days)
        {
            var startDate = DateTime.Now.Date.AddDays(-days);
            return await _context.ChildSleepRecords
                .Include(s => s.Child)
                .Include(s => s.SleepReference)
                .Where(s => s.ChildId == childId && s.SleepDate >= startDate)
                .OrderByDescending(s => s.SleepDate)
                .ToListAsync();
        }
    }
}