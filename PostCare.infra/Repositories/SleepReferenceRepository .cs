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
    /// <summary>
    /// Repository implementation for Sleep Reference
    /// </summary>
    public class SleepReferenceRepository : ISleepReferenceRepository
    {
        private readonly PostCareDbContext _context;

        public SleepReferenceRepository(PostCareDbContext context)
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

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}
