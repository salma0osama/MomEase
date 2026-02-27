using Microsoft.EntityFrameworkCore;
using MomEase.core.Entities;
using MomEase.core.Interfaces;
using MomEase.infra.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MomEase.infra.Repositories
{
    public class ScoreLevelRepository : IScoreLevelRepository
    {
        private readonly MomEaseDbContext _context;

        public ScoreLevelRepository(MomEaseDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ScoreLevel>> GetAllByAssessmentAsync(int assessmentId)
        {
            return await _context.ScoreLevels
                .AsNoTracking()
                .Where(sl => sl.AssessmentId == assessmentId)
                .OrderBy(sl => sl.MinScore)
                .ToListAsync();
        }

        public async Task<ScoreLevel?> GetByIdAsync(int assessmentId, int levelId)
        {
            return await _context.ScoreLevels
                .AsNoTracking()
                .FirstOrDefaultAsync(sl => sl.AssessmentId == assessmentId && sl.LevelId == levelId);
        }

        public async Task<ScoreLevel?> GetLevelByScoreAsync(int assessmentId, int score)
        {
            return await _context.ScoreLevels
                .AsNoTracking()
                .FirstOrDefaultAsync(sl =>
                    sl.AssessmentId == assessmentId &&
                    score >= sl.MinScore &&
                    score <= sl.MaxScore);
        }

        public async Task<ScoreLevel> CreateAsync(ScoreLevel scoreLevel)
        {
            _context.ScoreLevels.Add(scoreLevel);
            await _context.SaveChangesAsync();
            return scoreLevel;
        }

        public async Task<ScoreLevel> UpdateAsync(ScoreLevel scoreLevel)
        {
            _context.ScoreLevels.Update(scoreLevel);
            await _context.SaveChangesAsync();
            return scoreLevel;
        }

        public async Task<bool> DeleteAsync(int assessmentId, int levelId)
        {
            var level = await _context.ScoreLevels
                .FirstOrDefaultAsync(sl => sl.AssessmentId == assessmentId && sl.LevelId == levelId);

            if (level == null) return false;

            _context.ScoreLevels.Remove(level);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> AssessmentExistsAsync(int assessmentId)
        {
            return await _context.Assessments.AnyAsync(a => a.AssessmentId == assessmentId);
        }
    }
}