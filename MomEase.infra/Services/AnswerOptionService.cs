using MomEase.core.DTOS.CreateAnswerOptionsDto;
using MomEase.core.Entities;
using MomEase.core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MomEase.infra.Services
{
    public class AnswerOptionService : IAnswerOptionService
    {
        private readonly IAnswerOptionRepository _repo;

        public AnswerOptionService(IAnswerOptionRepository repo)
        {
            _repo = repo;
        }

        public async Task<IEnumerable<AnswerOptionDto>> GetAllByQuestionAsync(int questionId)
        {
            var options = await _repo.GetAllByQuestionAsync(questionId);
            return options.Select(MapToDto);
        }

        public async Task<AnswerOptionDto?> GetByIdAsync(int questionId, int optionId)
        {
            var option = await _repo.GetByIdAsync(questionId, optionId);
            if (option == null) return null;
            return MapToDto(option);
        }

        public async Task<(AnswerOptionDto? result, string? error)> CreateAsync(int questionId, CreateAnswerOptionDto dto)
        {
            if (!await _repo.QuestionExistsAsync(questionId))
                return (null, $"Question {questionId} not found.");

            var entity = new AnswerOption
            {
                QuestionId = questionId,
                OptionText = dto.OptionText,
                Score = dto.Score,
                OptionOrder = dto.OptionOrder
            };

            var created = await _repo.CreateAsync(entity);
            return (MapToDto(created), null);
        }

        public async Task<(AnswerOptionDto? result, string? error)> UpdateAsync(int questionId, int optionId, UpdateAnswerOptionDto dto)
        {
            var entity = await _repo.GetByIdAsync(questionId, optionId);
            if (entity == null)
                return (null, $"Option {optionId} not found in Question {questionId}.");

            if (dto.OptionText != null) entity.OptionText = dto.OptionText;
            if (dto.Score != null) entity.Score = dto.Score.Value;
            if (dto.OptionOrder != null) entity.OptionOrder = dto.OptionOrder.Value;

            var updated = await _repo.UpdateAsync(entity);
            return (MapToDto(updated), null);
        }

        public async Task<bool> DeleteAsync(int questionId, int optionId)
        {
            return await _repo.DeleteAsync(questionId, optionId);
        }

        // ── MAPPER ────────────────────────────────────────

        private static AnswerOptionDto MapToDto(AnswerOption o) => new()
        {
            OptionId = o.OptionId,
            QuestionId = o.QuestionId,
            OptionText = o.OptionText,
            Score = o.Score,
            OptionOrder = o.OptionOrder
        };
    }
}
