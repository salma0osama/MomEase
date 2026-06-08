using MomEase.core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MomEase.infra.Repositories
{
    public interface ICryAnalysisRepository
    {
        Task<CryAnalyses> AddAsync(CryAnalyses analysis);
        Task<CryAnalyses> GetByIdAsync(int id);
        Task<List<CryAnalyses>> GetByUserIdAsync(int userId);
        Task<List<CryAnalyses>> GetByChildIdAsync(int childId);
        Task DeleteAsync(int id);
    }
}
