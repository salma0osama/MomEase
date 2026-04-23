using MomEase.core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MomEase.core.Interfaces
{
    public interface IScoreLevelRepository
    {
        Task<IEnumerable<ScoreLevel>> GetAllByAssessmentAsync(int assessmentId);
        Task<ScoreLevel?> GetByIdAsync(int assessmentId, int levelId);
        Task<ScoreLevel?> GetLevelByScoreAsync(int assessmentId, int score);
        Task<ScoreLevel> CreateAsync(ScoreLevel scoreLevel);
        Task<ScoreLevel> UpdateAsync(ScoreLevel scoreLevel);
        Task<ScoreLevel?> GetByScoreAsync(int assessmentId, int score);
        Task<bool> DeleteAsync(int assessmentId, int levelId);
        Task<bool> AssessmentExistsAsync(int assessmentId);
    }
}