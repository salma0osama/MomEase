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
    public class GrowthReportRepository : IGrowthReportRepository
    {
        private readonly MomEaseDbContext _context;

        public GrowthReportRepository(MomEaseDbContext context)
        {
            _context = context;
        }

        public async Task<GrowthReports> CreateAsync(GrowthReports report)
        {
            await _context.GrowthReports.AddAsync(report);
            await _context.SaveChangesAsync();
            return report;
        }

        public async Task<GrowthReports> GetByIdAsync(int reportId)
        {
            return await _context.GrowthReports
                .Include(r => r.Child)
                .FirstOrDefaultAsync(r => r.ReportId == reportId);
        }

        public async Task<IEnumerable<GrowthReports>> GetByChildIdAsync(int childId)
        {
            return await _context.GrowthReports
                .Where(r => r.ChildId == childId)
                .OrderByDescending(r => r.PeriodEnd)
                .ToListAsync();
        }

        public async Task<GrowthReports> GetLatestByChildIdAsync(int childId)
        {
            return await _context.GrowthReports
                .Where(r => r.ChildId == childId)
                .OrderByDescending(r => r.PeriodEnd)
                .FirstOrDefaultAsync();
        }

        public async Task<bool> DeleteAsync(int reportId)
        {
            var report = await _context.GrowthReports.FindAsync(reportId);
            if (report == null) return false;

            _context.GrowthReports.Remove(report);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> BelongsToChildAsync(int reportId, int childId)
        {
            return await _context.GrowthReports
                .AnyAsync(r => r.ReportId == reportId && r.ChildId == childId);
        }
    }
}
