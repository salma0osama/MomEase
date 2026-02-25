using MomEase.core.DTOS.QuestionsDto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MomEase.core.Interfaces
{
    public interface IQuestionService
    {
        Task<IEnumerable<QuestionDto>> GetAllByAssessmentAsync(int assessmentId);
        Task<QuestionDto?> GetByIdAsync(int assessmentId, int questionId);
        Task<(QuestionDto? result, string? error)> CreateAsync(int assessmentId, CreateQuestionDto dto);
        Task<(QuestionDto? result, string? error)> UpdateAsync(int assessmentId, int questionId, UpdateQuestionDto dto);
        Task<bool> DeleteAsync(int assessmentId, int questionId);
    }
}
