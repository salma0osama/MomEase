using MomEase.core.DTOS.GrowthTracking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MomEase.core.Interfaces
{
    public interface IGrowthRecordService
    {
        Task<GrowthRecordDto> CreateAsync(int childId, int userId, CreateGrowthRecordDto dto);
        Task<GrowthRecordDto> GetByIdAsync(int growthId, int childId, int userId);
        Task<IEnumerable<GrowthRecordDto>> GetAllAsync(int childId, int userId);
        Task<GrowthRecordDto> UpdateAsync(int growthId, int childId, int userId, UpdateGrowthRecordDto dto);
        Task<bool> DeleteAsync(int growthId, int childId, int userId);
        Task<GrowthChartDto> GetChartDataAsync(int childId, int userId);
    }
}
