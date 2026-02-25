using Microsoft.EntityFrameworkCore;
using MomEase.core.Entities;
using MomEase.core.Interfaces;
using MomEase.infra.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MomEase.infra.Repositories
{
    public class AnswerOptionRepository : IAnswerOptionRepository
    {
        private readonly MomEaseDbContext _context;

        public AnswerOptionRepository(MomEaseDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<AnswerOption>> GetAllByQuestionAsync(int questionId)
        {
            return await _context.AnswerOptions
                .AsNoTracking()
                .Where(o => o.QuestionId == questionId)
                .OrderBy(o => o.OptionOrder)
                .ToListAsync();
        }

        public async Task<AnswerOption?> GetByIdAsync(int questionId, int optionId)
        {
            return await _context.AnswerOptions
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.QuestionId == questionId && o.OptionId == optionId);
        }

        public async Task<AnswerOption> CreateAsync(AnswerOption option)
        {
            _context.AnswerOptions.Add(option);
            await _context.SaveChangesAsync();
            return option;
        }

        public async Task<AnswerOption> UpdateAsync(AnswerOption option)
        {
            _context.AnswerOptions.Update(option);
            await _context.SaveChangesAsync();
            return option;
        }

        public async Task<bool> DeleteAsync(int questionId, int optionId)
        {
            var option = await _context.AnswerOptions
                .FirstOrDefaultAsync(o => o.QuestionId == questionId && o.OptionId == optionId);

            if (option == null) return false;

            _context.AnswerOptions.Remove(option);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> QuestionExistsAsync(int questionId)
        {
            return await _context.Questions.AnyAsync(q => q.QuestionId == questionId);
        }
    }
}
