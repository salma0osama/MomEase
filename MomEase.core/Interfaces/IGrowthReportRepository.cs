using MomEase.core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MomEase.core.Interfaces
{
    public interface IGrowthReportRepository
    {
        Task<GrowthReports> CreateAsync(GrowthReports report);
        Task<GrowthReports> GetByIdAsync(int reportId);
        Task<IEnumerable<GrowthReports>> GetByChildIdAsync(int childId);
        Task<GrowthReports> GetLatestByChildIdAsync(int childId);
        Task<bool> DeleteAsync(int reportId);
        Task<bool> BelongsToChildAsync(int reportId, int childId);
    }
}
