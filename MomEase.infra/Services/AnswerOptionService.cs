using Microsoft.AspNetCore.Http;
using MomEase.core.DTOS.CreateAnswerOptionsDto;
using MomEase.core.Entities;
using MomEase.core.Interfaces;
using MomEase.infra.Helpers;
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
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AnswerOptionService(
            IAnswerOptionRepository repo,
            IHttpContextAccessor httpContextAccessor)
        {
            _repo = repo;
            _httpContextAccessor = httpContextAccessor;
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
                OptionTextAr = dto.OptionTextAr,
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
            if (dto.OptionTextAr != null) entity.OptionTextAr = dto.OptionTextAr;
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

        private AnswerOptionDto MapToDto(AnswerOption o)
        {
            var lang = LanguageHelper.GetLang(_httpContextAccessor);

            return new AnswerOptionDto
            {
                OptionId = o.OptionId,
                QuestionId = o.QuestionId,
                OptionText = LanguageHelper.GetLocalized(o.OptionTextAr, o.OptionText, lang),
                Score = o.Score,
                OptionOrder = o.OptionOrder
            };
        }
    }
}
