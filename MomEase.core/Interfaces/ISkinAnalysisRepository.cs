using MomEase.core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MomEase.core.Interfaces
{
    public interface ISkinAnalysisRepository
    {
        Task<SkinAnalyses> AddAsync(SkinAnalyses analysis);
        Task<SkinAnalyses> GetByIdAsync(int id);
        Task<List<SkinAnalyses>> GetByUserIdAsync(int userId);
        Task<List<SkinAnalyses>> GetByChildIdAsync(int childId);
        Task DeleteAsync(int id);
    }
}
