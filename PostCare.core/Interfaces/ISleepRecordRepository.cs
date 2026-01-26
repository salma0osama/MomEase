using PostCare.core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PostCare.core.Interfaces
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
    }
}
