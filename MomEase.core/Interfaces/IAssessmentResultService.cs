using MomEase.core.DTOS.AssessmentDto;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MomEase.core.Interfaces
{
    public interface IAssessmentResultService
    {
        Task<(AssessmentResultDto? result, string? error)> SubmitAssessmentAsync(int userId, int assessmentId, SubmitAssessmentDto dto);
        Task<IEnumerable<AssessmentResultDto>> GetUserResultsAsync(int userId);
        Task<AssessmentResultDto?> GetResultByIdAsync(int userId, int resultId);
        Task<AssessmentResultDetailsDto?> GetResultDetailsAsync(int userId, int resultId);
        Task<bool> DeleteResultAsync(int userId, int resultId);
        Task<AssessmentResultDto?> GetLatestResultAsync(int userId);
    }
}