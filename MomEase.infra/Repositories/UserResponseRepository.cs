using Microsoft.EntityFrameworkCore;
using MomEase.core.Entities;
using MomEase.core.Interfaces;
using MomEase.infra.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MomEase.infra.Repositories
{
    public class UserResponseRepository : IUserResponseRepository
    {
        private readonly MomEaseDbContext _context;

        public UserResponseRepository(MomEaseDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<UserResponse>> GetByResultIdAsync(int resultId)
        {
            return await _context.UserResponses
                .AsNoTracking()
                .Include(ur => ur.Question)
                .Include(ur => ur.AnswerOption)
                .Where(ur => ur.ResultId == resultId)
                .OrderBy(ur => ur.Question.QuestionOrder)
                .ToListAsync();
        }

        public async Task<UserResponse?> GetByIdAsync(int resultId, int responseId)
        {
            return await _context.UserResponses
                .AsNoTracking()
                .Include(ur => ur.Question)
                .Include(ur => ur.AnswerOption)
                .FirstOrDefaultAsync(ur => ur.ResultId == resultId && ur.ResponseId == responseId);
        }

        public async Task<UserResponse> CreateAsync(UserResponse response)
        {
            _context.UserResponses.Add(response);
            await _context.SaveChangesAsync();
            return response;
        }
    }
}