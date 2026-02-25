using MomEase.core.DTOS.QuestionsDto;
using MomEase.core.Entities;
using MomEase.core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MomEase.infra.Services
{
    public class QuestionService : IQuestionService
    {
        private readonly IQuestionRepository _repo;

        public QuestionService(IQuestionRepository repo)
        {
            _repo = repo;
        }

        public async Task<IEnumerable<QuestionDto>> GetAllByAssessmentAsync(int assessmentId)
        {
            var questions = await _repo.GetAllByAssessmentAsync(assessmentId);
            return questions.Select(MapToDto);
        }

        public async Task<QuestionDto?> GetByIdAsync(int assessmentId, int questionId)
        {
            var question = await _repo.GetByIdAsync(assessmentId, questionId);
            if (question == null) return null;
            return MapToDto(question);
        }

        public async Task<(QuestionDto? result, string? error)> CreateAsync(int assessmentId, CreateQuestionDto dto)
        {
            if (!await _repo.AssessmentExistsAsync(assessmentId))
                return (null, $"Assessment {assessmentId} not found.");

            var entity = new Question
            {
                AssessmentId = assessmentId,
                QuestionText = dto.QuestionText,
                QuestionOrder = dto.QuestionOrder,
                IsReverse = dto.IsReverse
            };

            var created = await _repo.CreateAsync(entity);
            return (MapToDto(created), null);
        }

        public async Task<(QuestionDto? result, string? error)> UpdateAsync(int assessmentId, int questionId, UpdateQuestionDto dto)
        {
            var entity = await _repo.GetByIdAsync(assessmentId, questionId);
            if (entity == null)
                return (null, $"Question {questionId} not found in Assessment {assessmentId}.");

            if (dto.QuestionText != null) entity.QuestionText = dto.QuestionText;
            if (dto.QuestionOrder != null) entity.QuestionOrder = dto.QuestionOrder.Value;
            if (dto.IsReverse != null) entity.IsReverse = dto.IsReverse.Value;

            var updated = await _repo.UpdateAsync(entity);
            return (MapToDto(updated), null);
        }

        public async Task<bool> DeleteAsync(int assessmentId, int questionId)
        {
            return await _repo.DeleteAsync(assessmentId, questionId);
        }

        // ── MAPPER ────────────────────────────────────────

        private static QuestionDto MapToDto(Question q) => new()
        {
            QuestionId = q.QuestionId,
            AssessmentId = q.AssessmentId,
            QuestionText = q.QuestionText,
            QuestionOrder = q.QuestionOrder,
            IsReverse = q.IsReverse
        };
    }
}
