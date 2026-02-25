using MomEase.core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MomEase.core.Interfaces
{
    public interface IQuestionRepository
    {
        Task<IEnumerable<Question>> GetAllByAssessmentAsync(int assessmentId);
        Task<Question?> GetByIdAsync(int assessmentId, int questionId);
        Task<Question> CreateAsync(Question question);
        Task<Question> UpdateAsync(Question question);
        Task<bool> DeleteAsync(int assessmentId, int questionId);
        Task<bool> AssessmentExistsAsync(int assessmentId);
    }
}
