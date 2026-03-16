using Microsoft.Extensions.Logging;
using MomEase.core.DTOS.AssessmentDto;
using MomEase.core.Entities;
using MomEase.core.Interfaces;

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
        private readonly ILogger<AssessmentResultService> _logger;

        public AssessmentResultService(
            IAssessmentResultRepository resultRepo,
            IUserResponseRepository responseRepo,
            IQuestionRepository questionRepo,
            IAnswerOptionRepository optionRepo,
            IScoreLevelRepository scoreLevelRepo,
            INotificationService notificationService,
            IMentalHealthFollowUpService followUpService,
            ILogger<AssessmentResultService> logger)
        {
            _resultRepo = resultRepo;
            _responseRepo = responseRepo;
            _questionRepo = questionRepo;
            _optionRepo = optionRepo;
            _scoreLevelRepo = scoreLevelRepo;
            _notificationService = notificationService;
            _followUpService = followUpService;
            _logger = logger;
        }

        public async Task<(AssessmentResultDto? result, string? error)> SubmitAssessmentAsync(
            int userId,
            int assessmentId,
            SubmitAssessmentDto dto)
        {
            try
            {
                _logger.LogInformation("📝 User {UserId} submitting assessment {AssessmentId}", userId, assessmentId);

                // Validate answers
                if (dto.Answers == null || !dto.Answers.Any())
                    return (null, "No answers provided");

                // Calculate total score
                int totalScore = 0;
                foreach (var answer in dto.Answers)
                {
                    var question = await _questionRepo.GetByIdAsync(assessmentId, answer.QuestionId);
                    if (question == null)
                        return (null, $"Question {answer.QuestionId} not found");

                    var option = await _optionRepo.GetByIdAsync(answer.QuestionId, answer.OptionId);
                    if (option == null)
                        return (null, $"Option {answer.OptionId} not found for question {answer.QuestionId}");

                    // Handle reverse scoring
                    int score = question.IsReverse ? (5 - option.Score) : option.Score;
                    totalScore += score;
                }

                _logger.LogInformation("📊 Total score calculated: {TotalScore}", totalScore);

                // Find matching score level
                var scoreLevel = await _scoreLevelRepo.GetLevelByScoreAsync(assessmentId, totalScore);
                if (scoreLevel == null)
                    return (null, $"No score level found for score {totalScore}");

                // Create assessment result
                var result = new AssessmentResult
                {
                    UserId = userId,
                    AssessmentId = assessmentId,
                    TotalScore = totalScore,
                    LevelId = scoreLevel.LevelId,
                    CompletedAt = DateTime.Now
                };

                var savedResult = await _resultRepo.CreateAsync(result);

                // Save individual responses
                foreach (var answer in dto.Answers)
                {
                    var question = await _questionRepo.GetByIdAsync(assessmentId, answer.QuestionId);
                    var option = await _optionRepo.GetByIdAsync(answer.QuestionId, answer.OptionId);

                    int score = question!.IsReverse ? (5 - option!.Score) : option!.Score;

                    var response = new UserResponse
                    {
                        ResultId = savedResult.ResultId,
                        QuestionId = answer.QuestionId,
                        OptionId = answer.OptionId,
                        ComputedScore = score
                    };

                    await _responseRepo.CreateAsync(response);
                }

                // ✅ إرسال Notification حسب النتيجة
                await SendAssessmentResultNotificationAsync(userId, scoreLevel, totalScore);

                // ✅ إنشاء Follow-up Plan
                try
                {
                    await _followUpService.CreateFollowUpPlanAsync(
                        userId,
                        savedResult.ResultId,
                        scoreLevel.LevelName);

                    _logger.LogInformation(
                        "✅ Follow-up plan created for user {UserId}",
                        userId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "⚠️ Failed to create follow-up plan for user {UserId}, but assessment saved successfully",
                        userId);
                    // لا نرمي Exception - الـ Assessment اتحفظ بنجاح
                }

                _logger.LogInformation("✅ Assessment submitted successfully. ResultId: {ResultId}", savedResult.ResultId);

                return (new AssessmentResultDto
                {
                    ResultId = savedResult.ResultId,
                    AssessmentId = savedResult.AssessmentId,
                    TotalScore = savedResult.TotalScore,
                    LevelName = scoreLevel.LevelName,
                    Advice = scoreLevel.Advice,
                    CompletedAt = savedResult.CompletedAt
                }, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error submitting assessment");
                throw new InvalidOperationException($"Failed to submit assessment: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<AssessmentResultDto>> GetUserResultsAsync(int userId)
        {
            var results = await _resultRepo.GetByUserIdAsync(userId);
            return results.Select(r => new AssessmentResultDto
            {
                ResultId = r.ResultId,
                AssessmentId = r.AssessmentId,
                TotalScore = r.TotalScore,
                LevelName = r.ScoreLevel?.LevelName,
                Advice = r.ScoreLevel?.Advice,
                CompletedAt = r.CompletedAt
            });
        }

        public async Task<AssessmentResultDto?> GetResultByIdAsync(int userId, int resultId)
        {
            var result = await _resultRepo.GetByIdAsync(userId, resultId);
            if (result == null) return null;

            return new AssessmentResultDto
            {
                ResultId = result.ResultId,
                AssessmentId = result.AssessmentId,
                TotalScore = result.TotalScore,
                LevelName = result.ScoreLevel?.LevelName,
                Advice = result.ScoreLevel?.Advice,
                CompletedAt = result.CompletedAt
            };
        }

        public async Task<AssessmentResultDetailsDto?> GetResultDetailsAsync(int userId, int resultId)
        {
            var result = await _resultRepo.GetByIdWithResponsesAsync(userId, resultId);
            if (result == null) return null;

            return new AssessmentResultDetailsDto
            {
                ResultId = result.ResultId,
                AssessmentId = result.AssessmentId,
                TotalScore = result.TotalScore,
                LevelName = result.ScoreLevel?.LevelName,
                Advice = result.ScoreLevel?.Advice,
                CompletedAt = result.CompletedAt,
                Responses = result.UserResponses?.Select(r => new UserResponseForAssessmentDto
                {
                    ResponseId = r.ResponseId,
                    QuestionId = r.QuestionId,
                    QuestionText = r.Question?.QuestionText,
                    OptionId = r.OptionId,
                    OptionText = r.AnswerOption?.OptionText,
                    Score = r.ComputedScore
                }).ToList()
            };
        }

        public async Task<bool> DeleteResultAsync(int userId, int resultId)
        {
            return await _resultRepo.DeleteAsync(userId, resultId);
        }

        public async Task<AssessmentResultDto?> GetLatestResultAsync(int userId)
        {
            var result = await _resultRepo.GetLatestByUserIdAsync(userId);
            if (result == null) return null;

            return new AssessmentResultDto
            {
                ResultId = result.ResultId,
                AssessmentId = result.AssessmentId,
                TotalScore = result.TotalScore,
                LevelName = result.ScoreLevel?.LevelName,
                Advice = result.ScoreLevel?.Advice,
                CompletedAt = result.CompletedAt
            };
        }

        // ── PRIVATE HELPER: إرسال Notification حسب النتيجة ────────────────
        private async Task SendAssessmentResultNotificationAsync(
            int userId,
            ScoreLevel scoreLevel,
            int totalScore)
        {
            try
            {
                string title;
                string body;
                string type;

                if (scoreLevel.LevelName.Contains("Severe", StringComparison.OrdinalIgnoreCase))
                {
                    title = "🚨 Assessment Result - Severe";
                    body = $"Your assessment score ({totalScore} points) indicates severe symptoms. Please contact a mental health professional immediately.";
                    type = "AssessmentResultSevere";
                }
                else if (scoreLevel.LevelName.Contains("Moderate", StringComparison.OrdinalIgnoreCase))
                {
                    title = "⚠️ Assessment Result - Moderate";
                    body = $"Your assessment score ({totalScore} points) indicates moderate symptoms. We recommend speaking with a mental health specialist.";
                    type = "AssessmentResultModerate";
                }
                else if (scoreLevel.LevelName.Contains("Mild", StringComparison.OrdinalIgnoreCase))
                {
                    title = "ℹ️ Assessment Result - Mild";
                    body = $"Your assessment score ({totalScore} points) indicates mild symptoms. Take care of yourself and practice self-care.";
                    type = "AssessmentResultMild";
                }
                else // Minimal
                {
                    title = "✅ Assessment Result - Normal";
                    body = $"Your assessment score ({totalScore} points) is within the normal range. Keep taking care of your mental health.";
                    type = "AssessmentResultNormal";
                }

                await _notificationService.SendRealtimeNotificationAsync(
                    userId,
                    title,
                    body,
                    type,
                    null
                );

                _logger.LogInformation(
                    "✅ Assessment result notification sent to user {UserId}",
                    userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "❌ Failed to send assessment result notification to user {UserId}",
                    userId);
                // لا نرمي Exception - النتيجة اتحفظت بنجاح
            }
        }
    }
}