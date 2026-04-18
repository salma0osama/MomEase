using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using MomEase.core.DTOS.AssessmentDto;
using MomEase.core.Entities;
using MomEase.core.Interfaces;
using MomEase.infra.Helpers;

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

                var lang = LanguageHelper.GetLang(_httpContextAccessor);
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
                await SendAssessmentResultNotificationAsync(userId, scoreLevel, totalScore, lang);

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
            var lang = LanguageHelper.GetLang(_httpContextAccessor);
            var results = await _resultRepo.GetByUserIdAsync(userId);

            return results.Select(r => new AssessmentResultDto
            {
                ResultId = r.ResultId,
                AssessmentId = r.AssessmentId,
                TotalScore = r.TotalScore,
                LevelName = LanguageHelper.GetLocalized(
                    r.ScoreLevel?.LevelNameAr,
                    r.ScoreLevel?.LevelName,
                    lang),
                Advice = LanguageHelper.GetLocalized(
                    r.ScoreLevel?.AdviceAr,
                    r.ScoreLevel?.Advice,
                    lang),
                CompletedAt = r.CompletedAt
            });
        }

        public async Task<AssessmentResultDto?> GetResultByIdAsync(int userId, int resultId)
        {
            var lang = LanguageHelper.GetLang(_httpContextAccessor);
            var result = await _resultRepo.GetByIdAsync(userId, resultId);
            if (result == null) return null;

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
                CompletedAt = result.CompletedAt
            };
        }

        public async Task<AssessmentResultDetailsDto?> GetResultDetailsAsync(int userId, int resultId)
        {
            var lang = LanguageHelper.GetLang(_httpContextAccessor);
            var result = await _resultRepo.GetByIdWithResponsesAsync(userId, resultId);
            if (result == null) return null;

            return new AssessmentResultDetailsDto
            {
                ResultId = result.ResultId,
                AssessmentId = result.AssessmentId,
                TotalScore = result.TotalScore,
                LevelName = LanguageHelper.GetLocalized(
                    result.ScoreLevel?.LevelNameAr,
                    result.ScoreLevel?.LevelName ?? "",
                    lang),
                Advice = LanguageHelper.GetLocalized(
                    result.ScoreLevel?.AdviceAr,
                    result.ScoreLevel?.Advice ?? "",
                    lang),
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

        public async Task<bool> DeleteResultAsync(int userId, int resultId)
        {
            return await _resultRepo.DeleteAsync(userId, resultId);
        }

        public async Task<AssessmentResultDto?> GetLatestResultAsync(int userId)
        {
            var lang = LanguageHelper.GetLang(_httpContextAccessor);
            var result = await _resultRepo.GetLatestByUserIdAsync(userId);
            if (result == null) return null;

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
                CompletedAt = result.CompletedAt
            };
        }

        // ── PRIVATE HELPER: إرسال Notification حسب النتيجة ────────────────
        private async Task SendAssessmentResultNotificationAsync(
            int userId,
            ScoreLevel scoreLevel,
            int totalScore,
            string lang)
        {
            try
            {
                string title;
                string body;
                string type;

                // بنشيك على الـ LevelName الأصلي (غالباً بيكون بالانجليزي في الداتابيز كـ Identifier)
                if (scoreLevel.LevelName.Contains("Severe", StringComparison.OrdinalIgnoreCase))
                {
                    title = LanguageHelper.GetLocalized("🚨 نتيجة التقييم - حاد", "🚨 Assessment Result - Severe", lang);
                    body = LanguageHelper.GetLocalized(
                        $"درجة تقييمك ({totalScore} نقطة) تشير إلى وجود أعراض حادة. يرجى التواصل مع أخصائي فوراً.",
                        $"Your assessment score ({totalScore} points) indicates severe symptoms. Please contact a professional immediately.",
                        lang);
                    type = "AssessmentResultSevere";
                }
                else if (scoreLevel.LevelName.Contains("Moderate", StringComparison.OrdinalIgnoreCase))
                {
                    title = LanguageHelper.GetLocalized("⚠️ نتيجة التقييم - متوسط", "⚠️ Assessment Result - Moderate", lang);
                    body = LanguageHelper.GetLocalized(
                        $"درجة تقييمك ({totalScore} نقطة) تشير إلى أعراض متوسطة. ننصحك بالتحدث مع مختص.",
                        $"Your assessment score ({totalScore} points) indicates moderate symptoms. We recommend speaking with a specialist.",
                        lang);
                    type = "AssessmentResultModerate";
                }
                else if (scoreLevel.LevelName.Contains("Mild", StringComparison.OrdinalIgnoreCase))
                {
                    title = LanguageHelper.GetLocalized("ℹ️ نتيجة التقييم - طفيف", "ℹ️ Assessment Result - Mild", lang);
                    body = LanguageHelper.GetLocalized(
                        $"درجة تقييمك ({totalScore} نقطة) تشير إلى أعراض طفيفة. اهتم بنفسك ومارس الرعاية الذاتية.",
                        $"Your assessment score ({totalScore} points) indicates mild symptoms. Take care of yourself and practice self-care.",
                        lang);
                    type = "AssessmentResultMild";
                }
                else // Minimal / Normal
                {
                    title = LanguageHelper.GetLocalized("✅ نتيجة التقييم - طبيعي", "✅ Assessment Result - Normal", lang);
                    body = LanguageHelper.GetLocalized(
                        $"درجة تقييمك ({totalScore} نقطة) في النطاق الطبيعي. استمر في الاهتمام بصحتك النفسية.",
                        $"Your assessment score ({totalScore} points) is within the normal range. Keep taking care of your mental health.",
                        lang);
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
                    "✅ Assessment result notification sent to user {UserId} in language {Lang}",
                    userId, lang);
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