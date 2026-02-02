using PostCare.core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PostCare.core.Interfaces
{
    public interface IGrowthRecordRepository
    {
        Task<GrowthRecords> CreateAsync(GrowthRecords record);
        Task<GrowthRecords> GetByIdAsync(int growthId);
        Task<IEnumerable<GrowthRecords>> GetByChildIdAsync(int childId);
        Task<GrowthRecords> UpdateAsync(GrowthRecords record);
        Task<bool> DeleteAsync(int growthId);
        Task<bool> BelongsToChildAsync(int growthId, int childId);
    }
}
