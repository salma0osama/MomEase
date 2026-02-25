using MomEase.core.DTOS.CreateAnswerOptionsDto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MomEase.core.Interfaces
{
    public interface IAnswerOptionService
    {
        Task<IEnumerable<AnswerOptionDto>> GetAllByQuestionAsync(int questionId);
        Task<AnswerOptionDto?> GetByIdAsync(int questionId, int optionId);
        Task<(AnswerOptionDto? result, string? error)> CreateAsync(int questionId, CreateAnswerOptionDto dto);
        Task<(AnswerOptionDto? result, string? error)> UpdateAsync(int questionId, int optionId, UpdateAnswerOptionDto dto);
        Task<bool> DeleteAsync(int questionId, int optionId);
    }
}
