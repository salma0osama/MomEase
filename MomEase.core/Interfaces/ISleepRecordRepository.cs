using MomEase.core.Entities;

namespace MomEase.core.Interfaces
{
    public interface ISleepRecordRepository
    {
        Task<ChildSleepRecord> AddSleepRecordAsync(ChildSleepRecord record);
        Task<List<ChildSleepRecord>> GetChildSleepRecordsAsync(int childId);
        Task<ChildSleepRecord> GetSleepRecordByIdAsync(int recordId);
        Task<ChildSleepRecord> UpdateSleepRecordAsync(ChildSleepRecord record);
        Task<bool> DeleteSleepRecordAsync(ChildSleepRecord record);
        Task<List<ChildSleepRecord>> GetSleepRecordsByDateRangeAsync(int childId, DateTime startDate, DateTime endDate);
        Task<bool> IsSleepRecordOwnedByUserAsync(int recordId, int userId);

        // ✅ إضافة: منع السجلات المكررة
        Task<bool> ExistsForDateAsync(int childId, DateTime date, int? excludeRecordId = null);

        // ✅ إضافة: Get Last N Days
        Task<List<ChildSleepRecord>> GetLastNDaysAsync(int childId, int days);
    }
}