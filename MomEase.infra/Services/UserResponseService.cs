using Microsoft.AspNetCore.Http;
using MomEase.core.DTOS;
using MomEase.core.DTOS.AssessmentDto;
using MomEase.core.Interfaces;
using MomEase.infra.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MomEase.infra.Services
{
    public class UserResponseService : IUserResponseService
    {
        private readonly IUserResponseRepository _repo;
        private readonly IAssessmentResultRepository _resultRepo;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserResponseService(
            IUserResponseRepository repo,
            IAssessmentResultRepository resultRepo,
            IHttpContextAccessor httpContextAccessor)
        {
            _repo = repo;
            _resultRepo = resultRepo;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<IEnumerable<UserResponseForAssessmentDto>> GetResponsesByResultIdAsync(int userId, int resultId)
        {
            var lang = LanguageHelper.GetLang(_httpContextAccessor);
            // Verify ownership
            var result = await _resultRepo.GetByIdAsync(userId, resultId);
            if (result == null)
                throw new InvalidOperationException($"Assessment result {resultId} not found or access denied");

            var responses = await _repo.GetByResultIdAsync(resultId);

            return responses.Select(r => new UserResponseForAssessmentDto
            {
                ResponseId = r.ResponseId,
                QuestionId = r.QuestionId,
                QuestionText = LanguageHelper.GetLocalized(
                    r.Question?.QuestionTextAr,
                    r.Question?.QuestionText,
                    lang),
                OptionId = r.OptionId,
                OptionText = LanguageHelper.GetLocalized(
                    r.AnswerOption?.OptionTextAr,
                    r.AnswerOption?.OptionText,
                    lang),
                Score = r.ComputedScore
            });
        }

        public async Task<UserResponseForAssessmentDto?> GetResponseByIdAsync(int userId, int resultId, int responseId)
        {
            var lang = LanguageHelper.GetLang(_httpContextAccessor);
            // Verify ownership
            var result = await _resultRepo.GetByIdAsync(userId, resultId);
            if (result == null)
                return null;

            var response = await _repo.GetByIdAsync(resultId, responseId);
            if (response == null)
                return null;

            return new UserResponseForAssessmentDto
            {
                ResponseId = response.ResponseId,
                QuestionId = response.QuestionId,
                QuestionText = LanguageHelper.GetLocalized(
                    response.Question?.QuestionTextAr,
                    response.Question?.QuestionText,
                    lang),
                OptionId = response.OptionId,
                OptionText = LanguageHelper.GetLocalized(
                    response.AnswerOption?.OptionTextAr,
                    response.AnswerOption?.OptionText,
                    lang),
                Score = response.ComputedScore
            };
        }
    }
}