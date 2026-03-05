using MomEase.core.DTOS.GrowthReport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MomEase.core.Interfaces
{
    public interface IGrowthReportService
    {
        Task<GrowthReportDto> GenerateReportAsync(int childId, int userId, CreateGrowthReportDto dto);
        Task<GrowthReportDto> GetReportByIdAsync(int reportId, int childId, int userId);
        Task<IEnumerable<GrowthReportDto>> GetAllReportsAsync(int childId, int userId);
        Task<GrowthReportDto> GetLatestReportAsync(int childId, int userId);
        Task<bool> DeleteReportAsync(int reportId, int childId, int userId);
    }
}
