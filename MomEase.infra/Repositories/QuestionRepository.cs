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
    public class QuestionRepository : IQuestionRepository
    {
        private readonly MomEaseDbContext _context;

        public QuestionRepository(MomEaseDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Question>> GetAllByAssessmentAsync(int assessmentId)
        {
            return await _context.Questions
                .AsNoTracking()
                .Where(q => q.AssessmentId == assessmentId)
                .OrderBy(q => q.QuestionOrder)
                .ToListAsync();
        }

        public async Task<Question?> GetByIdAsync(int assessmentId, int questionId)
        {
            return await _context.Questions
                .AsNoTracking()
                .FirstOrDefaultAsync(q => q.AssessmentId == assessmentId && q.QuestionId == questionId);
        }

        public async Task<Question> CreateAsync(Question question)
        {
            _context.Questions.Add(question);
            await _context.SaveChangesAsync();
            return question;
        }

        public async Task<Question> UpdateAsync(Question question)
        {
            _context.Questions.Update(question);
            await _context.SaveChangesAsync();
            return question;
        }

        public async Task<bool> DeleteAsync(int assessmentId, int questionId)
        {
            var question = await _context.Questions
                .FirstOrDefaultAsync(q => q.AssessmentId == assessmentId && q.QuestionId == questionId);

            if (question == null) return false;

            _context.Questions.Remove(question);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> AssessmentExistsAsync(int assessmentId)
        {
            return await _context.Assessments.AnyAsync(a => a.AssessmentId == assessmentId);
        }
    }
}
