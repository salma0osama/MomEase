using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using MomEase.core.DTOS.AssessmentDto;
using MomEase.core.Entities;
using MomEase.core.Interfaces;
using MomEase.infra.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MomEase.infra.Services
{
    public class AssessmentResultService : IAssessmentResultService
    {
        private readonly IAssessmentResultRepository _resultRepo;
        private readonly IUserResponseRepository _responseRepo;
        private readonly IQuestionRepository _questionRepo;
        private readonly IAnswerOptionRepository _optionRepo;
        private readonly IScoreLevelRepository _scoreLevelRepo;
        private readonly INotificationService _notificationService;
        private readonly IMentalHealthFollowUpService _followUpService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<AssessmentResultService> _logger;

        public AssessmentResultService(
            IAssessmentResultRepository resultRepo,
            IUserResponseRepository responseRepo,
            IQuestionRepository questionRepo,
            IAnswerOptionRepository optionRepo,
            IScoreLevelRepository scoreLevelRepo,
            INotificationService notificationService,
            IMentalHealthFollowUpService followUpService,
            IHttpContextAccessor httpContextAccessor,
            ILogger<AssessmentResultService> logger)
        {
            _resultRepo = resultRepo;
            _responseRepo = responseRepo;
            _questionRepo = questionRepo;
            _optionRepo = optionRepo;
            _scoreLevelRepo = scoreLevelRepo;
            _notificationService = notificationService;
            _followUpService = followUpService;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        // ═══════════════════════════════════════════════════════
        // SUBMIT ASSESSMENT
        // ═══════════════════════════════════════════════════════
        public async Task<(AssessmentResultDto? result, string? error)> SubmitAssessmentAsync(
            int userId,
            int assessmentId,
            SubmitAssessmentDto dto)
        {
            try
            {
                _logger.LogInformation(
                    "📝 User {UserId} submitting Assessment {AssessmentId}",
                    userId, assessmentId);

                // 1️⃣ Validate Questions
                var questions = await _questionRepo.GetAllByAssessmentAsync(assessmentId);
                var questionIds = questions.Select(q => q.QuestionId).ToHashSet();

                if (dto.Answers.Any(a => !questionIds.Contains(a.QuestionId)))
                {
                    return (null, "Invalid question ID in answers.");
                }

                // 2️⃣ Calculate Total Score
                int totalScore = 0;
                var userResponses = new List<UserResponse>();

                foreach (var answer in dto.Answers)
                {
                    var option = await _optionRepo.GetByIdAsync(answer.QuestionId, answer.OptionId);
                    if (option == null || option.QuestionId != answer.QuestionId)
                    {
                        return (null, $"Invalid option {answer.OptionId} for question {answer.QuestionId}.");
                    }

                    var question = questions.FirstOrDefault(q => q.QuestionId == answer.QuestionId);
                    int computedScore = question?.IsReverse == true
                        ? (option.Score == 0 ? 3 : option.Score == 1 ? 2 : option.Score == 2 ? 1 : 0)
                        : option.Score;

                    totalScore += computedScore;

                    userResponses.Add(new UserResponse
                    {
                        QuestionId = answer.QuestionId,
                        OptionId = answer.OptionId,
                        ComputedScore = computedScore
                    });
                }

                _logger.LogInformation(
                    "✅ Total score calculated: {TotalScore} for user {UserId}",
                    totalScore, userId);

                // 3️⃣ Find Score Level
                var scoreLevel = await _scoreLevelRepo.GetByScoreAsync(assessmentId, totalScore);
                if (scoreLevel == null)
                {
                    return (null, $"No score level found for score {totalScore} in assessment {assessmentId}.");
                }

                // 4️⃣ Save Assessment Result
                var result = new AssessmentResult
                {
                    UserId = userId,
                    AssessmentId = assessmentId,
                    TotalScore = totalScore,
                    LevelId = scoreLevel.LevelId,
                    CompletedAt = DateTime.UtcNow
                };

                var savedResult = await _resultRepo.CreateAsync(result);

                // 5️⃣ Save User Responses
                foreach (var response in userResponses)
                {
                    response.ResultId = savedResult.ResultId;
                    await _responseRepo.CreateAsync(response);
                }

                _logger.LogInformation(
                    "💾 Assessment result saved with ID {ResultId}",
                    savedResult.ResultId);

                // 6️⃣ Send Notification
                await SendAssessmentResultNotificationAsync(userId, savedResult.ResultId, scoreLevel);

                // 7️⃣ Mental Health Follow-up (EPDS only)
                if (assessmentId == 1 && totalScore >= 13)
                {
                    _logger.LogInformation(
                        "🧠 Creating mental health follow-up for user {UserId} (Score: {Score})",
                        userId, totalScore);

                    await _followUpService.CreateFollowUpPlanAsync(
                        userId,
                        savedResult.ResultId,
                        scoreLevel.LevelName);
                }

                // 8️⃣ Return Result
                var lang = LanguageHelper.GetLang(_httpContextAccessor);
                savedResult.ScoreLevel = scoreLevel;

                return (MapToResultDto(savedResult, lang), null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "❌ Error submitting assessment for user {UserId}",
                    userId);
                throw;
            }
        }

        // ═══════════════════════════════════════════════════════
        // GET USER RESULTS
        // ═══════════════════════════════════════════════════════
        public async Task<IEnumerable<AssessmentResultDto>> GetUserResultsAsync(int userId)
        {
            var lang = LanguageHelper.GetLang(_httpContextAccessor);
            var results = await _resultRepo.GetByUserIdAsync(userId);

            return results.Select(r => MapToResultDto(r, lang));
        }

        // ═══════════════════════════════════════════════════════
        // GET RESULT BY ID
        // ═══════════════════════════════════════════════════════
        public async Task<AssessmentResultDto?> GetResultByIdAsync(int userId, int resultId)
        {
            var lang = LanguageHelper.GetLang(_httpContextAccessor);
            var result = await _resultRepo.GetByIdAsync(userId, resultId);
            if (result == null) return null;

            return MapToResultDto(result, lang);
        }

        // ═══════════════════════════════════════════════════════
        // GET RESULT DETAILS (with Responses)
        // ═══════════════════════════════════════════════════════
        public async Task<AssessmentResultDetailsDto?> GetResultDetailsAsync(int userId, int resultId)
        {
            var lang = LanguageHelper.GetLang(_httpContextAccessor);
            var result = await _resultRepo.GetByIdWithResponsesAsync(userId, resultId);
            if (result == null) return null;

            // ⬅️ Get static recommendations
            var recommendations = RecommendationsHelper.GetRecommendations(
                result.ScoreLevel?.LevelName ?? "Minimal",
                lang
            );

            return new AssessmentResultDetailsDto
            {
                ResultId = result.ResultId,
                AssessmentId = result.AssessmentId,
                TotalScore = result.TotalScore,
                LevelName = LanguageHelper.GetLocalized(
                    result.ScoreLevel?.LevelNameAr,
                    result.ScoreLevel?.LevelName,
                    lang),
                Advice = LanguageHelper.GetLocalized(
                    result.ScoreLevel?.AdviceAr,
                    result.ScoreLevel?.Advice,
                    lang),
                Recommendations = recommendations, // ⬅️ Static recommendations
                CompletedAt = result.CompletedAt,
                Responses = result.UserResponses?.Select(r => new UserResponseForAssessmentDto
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
                }).ToList()
            };
        }

        // ═══════════════════════════════════════════════════════
        // DELETE RESULT
        // ═══════════════════════════════════════════════════════
        public async Task<bool> DeleteResultAsync(int userId, int resultId)
        {
            return await _resultRepo.DeleteAsync(userId, resultId);
        }

        // ═══════════════════════════════════════════════════════
        // GET LATEST RESULT
        // ═══════════════════════════════════════════════════════
        public async Task<AssessmentResultDto?> GetLatestResultAsync(int userId)
        {
            var lang = LanguageHelper.GetLang(_httpContextAccessor);
            var result = await _resultRepo.GetLatestByUserIdAsync(userId);
            if (result == null) return null;

            return MapToResultDto(result, lang);
        }

        // ═══════════════════════════════════════════════════════
        // PRIVATE HELPERS
        // ═══════════════════════════════════════════════════════

        private AssessmentResultDto MapToResultDto(AssessmentResult result, string lang)
        {
            // ⬅️ Get static recommendations
            var recommendations = RecommendationsHelper.GetRecommendations(
                result.ScoreLevel?.LevelName ?? "Minimal",
                lang
            );

            return new AssessmentResultDto
            {
                ResultId = result.ResultId,
                AssessmentId = result.AssessmentId,
                TotalScore = result.TotalScore,
                LevelName = LanguageHelper.GetLocalized(
                    result.ScoreLevel?.LevelNameAr,
                    result.ScoreLevel?.LevelName,
                    lang),
                Advice = LanguageHelper.GetLocalized(
                    result.ScoreLevel?.AdviceAr,
                    result.ScoreLevel?.Advice,
                    lang),
                Recommendations = recommendations, // ⬅️ Static recommendations
                CompletedAt = result.CompletedAt
            };
        }

        private async Task SendAssessmentResultNotificationAsync(int userId, int resultId, ScoreLevel scoreLevel)
        {
            try
            {
                var lang = LanguageHelper.GetLang(_httpContextAccessor);

                string emoji = scoreLevel.LevelName switch
                {
                    "Severe" => "🚨",
                    "Moderate" => "⚠️",
                    "Mild" => "💛",
                    _ => "✅"
                };

                string titleEn = $"{emoji} Assessment Result - {scoreLevel.LevelName}";
                string titleAr = $"{emoji} نتيجة الاختبار - {scoreLevel.LevelNameAr ?? scoreLevel.LevelName}";

                string title = LanguageHelper.GetLocalized(titleAr, titleEn, lang);
                string message = LanguageHelper.GetLocalized(scoreLevel.AdviceAr, scoreLevel.Advice, lang);

                await _notificationService.SendRealtimeNotificationAsync(
                    userId,
                    title,
                    message,
                    "AssessmentResult",
                   relatedEntityId: resultId,
                    actionUrl: $"/assessments/results/{resultId}"
                );

                _logger.LogInformation(
                    "📤 Assessment result notification sent to user {UserId}",
                    userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "❌ Failed to send assessment result notification to user {UserId}",
                    userId);
            }
        }
    }
}