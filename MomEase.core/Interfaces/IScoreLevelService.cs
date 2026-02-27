using MomEase.core.DTOS.AssessmentDto;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MomEase.core.Interfaces
{
    public interface IScoreLevelService
    {
        Task<IEnumerable<ScoreLevelDto>> GetAllByAssessmentAsync(int assessmentId);
        Task<ScoreLevelDto?> GetByIdAsync(int assessmentId, int levelId);
        Task<(ScoreLevelDto? result, string? error)> CreateAsync(int assessmentId, CreateScoreLevelDto dto);
        Task<(ScoreLevelDto? result, string? error)> UpdateAsync(int assessmentId, int levelId, UpdateScoreLevelDto dto);
        Task<bool> DeleteAsync(int assessmentId, int levelId);
    }
}