using PostCare.core.Entities;
using PostCare.core.Enums;

namespace PostCare.core.Interfaces
{
    /// <summary>
    /// Repository interface for Feeding Reference operations
    /// </summary>
    public interface IFeedingReferenceRepository
    {
        Task<FeedingReference?> GetByIdAsync(int feedingRefId);
        Task<FeedingReference?> GetByAgeAndTypeAsync(int ageInMonths, FeedingTypeForBaby feedingType);
        Task<List<FeedingReference>> GetByAgeAsync(int ageInMonths);
        Task<List<FeedingReference>> GetAllAsync();
        Task<int> SaveChangesAsync();
    }
}