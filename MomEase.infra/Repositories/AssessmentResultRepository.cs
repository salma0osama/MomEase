using Microsoft.EntityFrameworkCore;
using MomEase.core.Entities;
using MomEase.core.Interfaces;
using MomEase.infra.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MomEase.infra.Repositories
{
    public class AssessmentResultRepository : IAssessmentResultRepository
    {
        private readonly MomEaseDbContext _context;

        public AssessmentResultRepository(MomEaseDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<AssessmentResult>> GetByUserIdAsync(int userId)
        {
            return await _context.AssessmentResults
                .AsNoTracking()
                .Include(ar => ar.ScoreLevel)
                .Include(ar => ar.Assessment)
                .Where(ar => ar.UserId == userId)
                .OrderByDescending(ar => ar.CompletedAt)
                .ToListAsync();
        }

        public async Task<AssessmentResult?> GetByIdAsync(int userId, int resultId)
        {
            return await _context.AssessmentResults
                .AsNoTracking()
                .Include(ar => ar.ScoreLevel)
                .Include(ar => ar.Assessment)
                .FirstOrDefaultAsync(ar => ar.UserId == userId && ar.ResultId == resultId);
        }

        public async Task<AssessmentResult?> GetByIdWithResponsesAsync(int userId, int resultId)
        {
            return await _context.AssessmentResults
                .AsNoTracking()
                .Include(ar => ar.ScoreLevel)
                .Include(ar => ar.Assessment)
                .Include(ar => ar.UserResponses)
                    .ThenInclude(ur => ur.Question)
                .Include(ar => ar.UserResponses)
                    .ThenInclude(ur => ur.AnswerOption)
                .FirstOrDefaultAsync(ar => ar.UserId == userId && ar.ResultId == resultId);
        }

        public async Task<AssessmentResult?> GetLatestByUserIdAsync(int userId)
        {
            return await _context.AssessmentResults
                .AsNoTracking()
                .Include(ar => ar.ScoreLevel)
                .Include(ar => ar.Assessment)
                .Where(ar => ar.UserId == userId)
                .OrderByDescending(ar => ar.CompletedAt)
                .FirstOrDefaultAsync();
        }

        public async Task<AssessmentResult> CreateAsync(AssessmentResult result)
        {
            _context.AssessmentResults.Add(result);
            await _context.SaveChangesAsync();
            return result;
        }

        public async Task<bool> DeleteAsync(int userId, int resultId)
        {
            var result = await _context.AssessmentResults
                .FirstOrDefaultAsync(ar => ar.UserId == userId && ar.ResultId == resultId);

            if (result == null) return false;

            _context.AssessmentResults.Remove(result);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}