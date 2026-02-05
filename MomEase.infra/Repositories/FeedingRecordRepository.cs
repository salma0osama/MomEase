using Microsoft.EntityFrameworkCore;
using MomEase.core.Entities;
using MomEase.core.Enums;
using MomEase.core.Interfaces;
using MomEase.infra.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MomEase.infra.Repositories
{
    /// <summary>
    /// Repository implementation for Child Feeding Records
    /// </summary>
    public class FeedingRecordRepository : IFeedingRecordRepository
    {
        private readonly MomEaseDbContext _context;

        public FeedingRecordRepository(MomEaseDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<List<ChildFeedingRecord>> GetByChildIdAsync(int childId)
        {
            return await _context.ChildFeedingRecords
                .Include(c => c.Child)
                .Include(c => c.FeedingReference)
                .Where(c => c.ChildId == childId)
                .OrderByDescending(c => c.FeedingDate)
                .ToListAsync();
        }

        public async Task<ChildFeedingRecord?> GetByIdAsync(int recordId)
        {
            return await _context.ChildFeedingRecords
                .Include(c => c.Child)
                .Include(c => c.FeedingReference)
                .FirstOrDefaultAsync(c => c.RecordId == recordId);
        }

        public async Task<ChildFeedingRecord?> GetByChildAndDateAsync(int childId, DateTime date)
        {
            var dateOnly = date.Date;
            return await _context.ChildFeedingRecords
                .FirstOrDefaultAsync(c =>
                    c.ChildId == childId &&
                    c.FeedingDate.Date == dateOnly);
        }

        public async Task<List<ChildFeedingRecord>> GetLastNDaysAsync(int childId, int days)
        {
            var startDate = DateTime.Now.Date.AddDays(-days);
            return await _context.ChildFeedingRecords
                .Include(c => c.Child)  
                .Include(c => c.FeedingReference)  
                .Where(c => c.ChildId == childId && c.FeedingDate >= startDate)
                .OrderByDescending(c => c.FeedingDate)
                .ToListAsync();
        }

        public async Task<List<ChildFeedingRecord>> GetByDateRangeAsync(int childId, DateTime startDate, DateTime endDate)
        {
            return await _context.ChildFeedingRecords
                .Where(c =>
                    c.ChildId == childId &&
                    c.FeedingDate >= startDate.Date &&
                    c.FeedingDate <= endDate.Date)
                .OrderByDescending(c => c.FeedingDate)
                .ToListAsync();
        }

        public async Task<List<ChildFeedingRecord>> GetCurrentMonthAsync(int childId)
        {
            var now = DateTime.Now;
            var startOfMonth = new DateTime(now.Year, now.Month, 1);
            var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

            return await GetByDateRangeAsync(childId, startOfMonth, endOfMonth);
        }

        public async Task<ChildFeedingRecord> AddAsync(ChildFeedingRecord feedingRecord)
        {
            if (feedingRecord == null)
                throw new ArgumentNullException(nameof(feedingRecord));

            await _context.ChildFeedingRecords.AddAsync(feedingRecord);
            return feedingRecord;
        }

        public Task UpdateAsync(ChildFeedingRecord feedingRecord)
        {
            if (feedingRecord == null)
                throw new ArgumentNullException(nameof(feedingRecord));

            _context.ChildFeedingRecords.Update(feedingRecord);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(ChildFeedingRecord feedingRecord)
        {
            if (feedingRecord == null)
                throw new ArgumentNullException(nameof(feedingRecord));

            _context.ChildFeedingRecords.Remove(feedingRecord);
            return Task.CompletedTask;
        }

        public async Task<bool> ExistsForDateAndTypeAsync(int childId, DateTime date, FeedingTypeForBaby feedingType, int? excludeRecordId = null)
        {
            var dateOnly = date.Date;
            var query = _context.ChildFeedingRecords
                .Where(c =>
                    c.ChildId == childId &&
                    c.FeedingDate.Date == dateOnly &&
                    c.FeedingTypeForBaby == feedingType);

            if (excludeRecordId.HasValue)
            {
                query = query.Where(c => c.RecordId != excludeRecordId.Value);
            }

            return await query.AnyAsync();
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}
