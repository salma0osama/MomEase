using MomEase.core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MomEase.core.Interfaces
{
    public interface IAnswerOptionRepository
    {
        Task<IEnumerable<AnswerOption>> GetAllByQuestionAsync(int questionId);
        Task<AnswerOption?> GetByIdAsync(int questionId, int optionId);
        Task<AnswerOption> CreateAsync(AnswerOption option);
        Task<AnswerOption> UpdateAsync(AnswerOption option);
        Task<bool> DeleteAsync(int questionId, int optionId);
        Task<bool> QuestionExistsAsync(int questionId);
    }
}
