using MomEase.core.DTOS.SleepRecordDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MomEase.core.Interfaces
{
    public interface ISleepRecordService
    {
        Task<SleepRecordDto> CreateSleepRecordAsync(int userId, CreateSleepRecordDto dto);
        Task<List<SleepRecordDto>> GetChildSleepRecordsAsync(int childId, int userId);
        Task<SleepRecordDto> GetSleepRecordByIdAsync(int recordId, int userId);
        Task<SleepRecordDto> UpdateSleepRecordAsync(int recordId, int userId, UpdateSleepRecordDto dto);
        Task<bool> DeleteSleepRecordAsync(int recordId, int userId);
        Task<SleepStatisticsDto> GetSleepStatisticsAsync(int childId, int userId);
        Task<WeeklySleepDto> GetWeeklySleepAsync(int childId, int userId);
        Task<MonthlySleepDto> GetMonthlySleepAsync(int childId, int userId);

    }
}
