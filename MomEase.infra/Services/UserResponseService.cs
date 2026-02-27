using MomEase.core.DTOS;
using MomEase.core.DTOS.AssessmentDto;
using MomEase.core.Interfaces;
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

        public UserResponseService(
            IUserResponseRepository repo,
            IAssessmentResultRepository resultRepo)
        {
            _repo = repo;
            _resultRepo = resultRepo;
        }

        public async Task<IEnumerable<UserResponseForAssessmentDto>> GetResponsesByResultIdAsync(int userId, int resultId)
        {
            // Verify ownership
            var result = await _resultRepo.GetByIdAsync(userId, resultId);
            if (result == null)
                throw new InvalidOperationException($"Assessment result {resultId} not found or access denied");

            var responses = await _repo.GetByResultIdAsync(resultId);

            return responses.Select(r => new UserResponseForAssessmentDto
            {
                ResponseId = r.ResponseId,
                QuestionId = r.QuestionId,
                QuestionText = r.Question?.QuestionText,
                OptionId = r.OptionId,
                OptionText = r.AnswerOption?.OptionText,
                Score = r.ComputedScore
            });
        }

        public async Task<UserResponseForAssessmentDto?> GetResponseByIdAsync(int userId, int resultId, int responseId)
        {
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
                QuestionText = response.Question?.QuestionText,
                OptionId = response.OptionId,
                OptionText = response.AnswerOption?.OptionText,
                Score = response.ComputedScore
            };
        }
    }
}