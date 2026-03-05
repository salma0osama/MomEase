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
    /// <summary>
    /// Repository implementation for Sleep Reference
    /// </summary>
    public class SleepReferenceRepository : ISleepReferenceRepository
    {
        private readonly MomEaseDbContext _context;

        public SleepReferenceRepository(MomEaseDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<SleepReference?> GetByIdAsync(int sleepRefId)
        {
            return await _context.SleepReferences
                .FirstOrDefaultAsync(s => s.SleepRefId == sleepRefId);
        }

        public async Task<SleepReference?> GetByAgeAsync(int ageInMonths)
        {
            return await _context.SleepReferences
                .FirstOrDefaultAsync(s =>
                    s.AgeMinMonths <= ageInMonths &&
                    s.AgeMaxMonths >= ageInMonths);
        }

        public async Task<List<SleepReference>> GetAllAsync()
        {
            return await _context.SleepReferences
                .OrderBy(s => s.AgeMinMonths)
                .ToListAsync();
        }
        public async Task<SleepReference> GetByAgeMonthsAsync(int ageMonths)
        {
            return await _context.SleepReferences
                .FirstOrDefaultAsync(r =>
                    r.AgeMinMonths <= ageMonths &&
                    r.AgeMaxMonths >= ageMonths);
        }
        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}
