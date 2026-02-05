using Microsoft.EntityFrameworkCore;
using MomEase.core.Entities;
using MomEase.core.Enums;
using MomEase.core.Interfaces;
using MomEase.infra.Data;

namespace MomEase.infra.Repositories
{
    /// <summary>
    /// Repository implementation for Feeding Reference
    /// </summary>
    public class FeedingReferenceRepository : IFeedingReferenceRepository
    {
        private readonly MomEaseDbContext _context;

        public FeedingReferenceRepository(MomEaseDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<FeedingReference?> GetByIdAsync(int feedingRefId)
        {
            return await _context.FeedingReferences
                .FirstOrDefaultAsync(f => f.FeedingRefId == feedingRefId);
        }

        public async Task<FeedingReference?> GetByAgeAndTypeAsync(int ageInMonths, FeedingTypeForBaby feedingType)
        {
            return await _context.FeedingReferences
                .FirstOrDefaultAsync(f =>
                    f.AgeMinMonths <= ageInMonths &&
                    f.AgeMaxMonths >= ageInMonths &&
                    f.FeedingTypeForBaby == feedingType);
        }

        public async Task<List<FeedingReference>> GetByAgeAsync(int ageInMonths)
        {
            return await _context.FeedingReferences
                .Where(f => f.AgeMinMonths <= ageInMonths && f.AgeMaxMonths >= ageInMonths)
                .ToListAsync();
        }

        public async Task<List<FeedingReference>> GetAllAsync()
        {
            return await _context.FeedingReferences.ToListAsync();
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}