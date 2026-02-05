using MomEase.core.Entities;
using MomEase.core.Enums;

namespace MomEase.core.Interfaces
{
    /// <summary>
    /// Repository interface for Child Feeding Record operations
    /// </summary>
    public interface IFeedingRecordRepository
    {
        Task<List<ChildFeedingRecord>> GetByChildIdAsync(int childId);
        Task<ChildFeedingRecord?> GetByIdAsync(int recordId);
        Task<ChildFeedingRecord?> GetByChildAndDateAsync(int childId, DateTime date);
        Task<List<ChildFeedingRecord>> GetLastNDaysAsync(int childId, int days);
        Task<List<ChildFeedingRecord>> GetByDateRangeAsync(int childId, DateTime startDate, DateTime endDate);
        Task<List<ChildFeedingRecord>> GetCurrentMonthAsync(int childId);
        Task<ChildFeedingRecord> AddAsync(ChildFeedingRecord feedingRecord);
        Task UpdateAsync(ChildFeedingRecord feedingRecord);
        Task DeleteAsync(ChildFeedingRecord feedingRecord);
        Task<bool> ExistsForDateAndTypeAsync(int childId, DateTime date, FeedingTypeForBaby feedingType, int? excludeRecordId = null);
        Task<int> SaveChangesAsync();
    }
}