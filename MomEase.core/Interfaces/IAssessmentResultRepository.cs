using MomEase.core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MomEase.core.Interfaces
{
    public interface IAssessmentResultRepository
    {
        Task<IEnumerable<AssessmentResult>> GetByUserIdAsync(int userId);
        Task<AssessmentResult?> GetByIdAsync(int userId, int resultId);
        Task<AssessmentResult?> GetByIdWithResponsesAsync(int userId, int resultId);
        Task<AssessmentResult?> GetLatestByUserIdAsync(int userId);
        Task<AssessmentResult> CreateAsync(AssessmentResult result);
        Task<bool> DeleteAsync(int userId, int resultId);
    }
}