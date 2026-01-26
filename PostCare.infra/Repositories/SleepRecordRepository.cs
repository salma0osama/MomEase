using Microsoft.EntityFrameworkCore;
using PostCare.core.Entities;
using PostCare.core.Interfaces;
using PostCare.infra.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PostCare.infra.Repositories
{
    public class SleepRecordRepository : ISleepRecordRepository
    {
        private readonly PostCareDbContext _context;

        public SleepRecordRepository(PostCareDbContext context)
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
                .Where(s => s.ChildId == childId && s.SleepDate >= startDate && s.SleepDate <= endDate)
                .OrderBy(s => s.SleepDate)
                .ToListAsync();
        }

        public async Task<bool> IsSleepRecordOwnedByUserAsync(int recordId, int userId)
        {
            return await _context.ChildSleepRecords
                .AnyAsync(s => s.RecordId == recordId && s.Child.UserId == userId);
        }
    }
}
