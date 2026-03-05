using Google;
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
    public class GrowthRecordRepository : IGrowthRecordRepository
    {
        private readonly MomEaseDbContext _context;

        public GrowthRecordRepository(MomEaseDbContext context)
        {
            _context = context;
        }

        public async Task<GrowthRecords> CreateAsync(GrowthRecords record)
        {
            await _context.GrowthRecords.AddAsync(record);
            await _context.SaveChangesAsync();
            return record;
        }

        public async Task<GrowthRecords> GetByIdAsync(int growthId)
        {
            return await _context.GrowthRecords
                .Include(g => g.Child)
                .FirstOrDefaultAsync(g => g.GrowthId == growthId);
        }

        public async Task<IEnumerable<GrowthRecords>> GetByChildIdAsync(int childId)
        {
            return await _context.GrowthRecords
                .Where(g => g.ChildId == childId)
                .OrderByDescending(g => g.RecordDate)
                .ToListAsync();
        }

        public async Task<GrowthRecords> UpdateAsync(GrowthRecords record)
        {
            _context.GrowthRecords.Update(record);
            await _context.SaveChangesAsync();
            return record;
        }

        public async Task<bool> DeleteAsync(int growthId)
        {
            var record = await _context.GrowthRecords.FindAsync(growthId);
            if (record == null) return false;

            _context.GrowthRecords.Remove(record);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> BelongsToChildAsync(int growthId, int childId)
        {
            return await _context.GrowthRecords
                .AnyAsync(g => g.GrowthId == growthId && g.ChildId == childId);
        }
        public async Task<IEnumerable<GrowthRecords>> GetByDateRangeAsync(
    int childId,
    DateTime startDate,
    DateTime endDate)
        {
            return await _context.GrowthRecords
                .Where(g => g.ChildId == childId
                         && g.RecordDate >= startDate
                         && g.RecordDate <= endDate)
                .OrderBy(g => g.RecordDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<GrowthRecords>> GetLastNRecordsAsync(
            int childId,
            int count)
        {
            return await _context.GrowthRecords
                .Where(g => g.ChildId == childId)
                .OrderByDescending(g => g.RecordDate)
                .Take(count)
                .OrderBy(g => g.RecordDate) // reverse back
                .ToListAsync();
        }
    }
}
