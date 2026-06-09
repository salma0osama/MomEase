using Microsoft.Extensions.Logging;
using MomEase.core.Entities;
using MomEase.core.Interfaces;
using MomEase.infra.Helpers;
using System;
using System.Threading.Tasks;

namespace MomEase.infra.Services
{
    public class MentalHealthFollowUpService : IMentalHealthFollowUpService
    {
        private readonly IMentalHealthFollowUpRepository _followUpRepo;
        private readonly IMentalHealthTipRepository _tipRepo;
        private readonly INotificationService _notificationService;
        private readonly IUserRepository _userRepo;
        private readonly ILogger<MentalHealthFollowUpService> _logger;

        public MentalHealthFollowUpService(
            IMentalHealthFollowUpRepository followUpRepo,
            IMentalHealthTipRepository tipRepo,
            INotificationService notificationService,
            IUserRepository userRepo,
            ILogger<MentalHealthFollowUpService> logger)
        {
            _followUpRepo = followUpRepo;
            _tipRepo = tipRepo;
            _notificationService = notificationService;
            _userRepo = userRepo;
            _logger = logger;
        }

        public async Task CreateFollowUpPlanAsync(
            int userId,
            int assessmentResultId,
            string severityLevel)
        {
            try
            {
                _logger.LogInformation(
                    "📋 Creating follow-up plan for user {UserId} with severity {SeverityLevel}",
                    userId, severityLevel);

                // إلغاء أي Follow-up سابق لم يكتمل
                var existingFollowUp = await _followUpRepo.GetActiveByUserIdAsync(userId);
                if (existingFollowUp != null)
                {
                    await _followUpRepo.CompleteFollowUpAsync(existingFollowUp.FollowUpId);
                    _logger.LogInformation(
                        "Completed previous follow-up {FollowUpId} for user {UserId}",
                        existingFollowUp.FollowUpId, userId);
                }

                // ⬅️ حدد المدد حسب شدة الاكتئاب
                var (assessmentInterval, tipInterval) = GetIntervals(severityLevel);

                var followUp = new MentalHealthFollowUp
                {
                    UserId = userId,
                    LastAssessmentResultId = assessmentResultId,
                    SeverityLevel = severityLevel,
                    NextAssessmentDate = DateTime.Now.AddHours(1).Add(assessmentInterval), // ⬅️ حسب الـ Severity
                    NextTipDate = DateTime.Now.AddHours(1).Add(tipInterval), // ⬅️ حسب الـ Severity
                    AssessmentReminderSent = false,
                    IsCompleted = false,
                    CreatedAt = DateTime.Now.AddHours(1)
                };

                await _followUpRepo.CreateAsync(followUp);

                _logger.LogInformation(
                    "✅ Follow-up plan created for severity '{SeverityLevel}'. Next assessment: {NextAssessment}, Next tip: {NextTip}",
                    severityLevel, followUp.NextAssessmentDate, followUp.NextTipDate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error creating follow-up plan for user {UserId}", userId);
                throw;
            }
        }

        public async Task SendDueAssessmentRemindersAsync()
        {
            try
            {
                var dueReminders = await _followUpRepo.GetDueAssessmentRemindersAsync();

                foreach (var followUp in dueReminders)
                {
                    try
                    {
                        _logger.LogInformation(
                            "📬 Sending assessment reminder to user {UserId}",
                            followUp.UserId);

                        var user = await _userRepo.GetByIdAsync(followUp.UserId);
                        var userLang = user?.PreferredLanguage ?? "en";

                        var title = LanguageHelper.GetLocalized(
                            "🧠 وقت تقييم صحتك النفسية",
                            "🧠 Mental Health Check-in Time",
                            userLang);

                        var message = GetAssessmentReminderMessage(followUp.SeverityLevel, userLang);

                        await _notificationService.SendRealtimeNotificationAsync(
                            followUp.UserId,
                            title,
                            message,
                            "MentalHealthAssessmentReminder",
                            followUp.LastAssessmentResultId,
                            actionUrl: $"/assessments/follow-up/{followUp.FollowUpId}"
                        );

                        followUp.AssessmentReminderSent = true;
                        await _followUpRepo.UpdateAsync(followUp);

                        _logger.LogInformation(
                            "✅ Assessment reminder sent to user {UserId} in {Language}",
                            followUp.UserId, userLang);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex,
                            "❌ Error sending assessment reminder to user {UserId}",
                            followUp.UserId);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error in SendDueAssessmentRemindersAsync");
            }
        }

        public async Task SendDueTipsAsync()
        {
            try
            {
                var dueTips = await _followUpRepo.GetDueTipsAsync();

                _logger.LogInformation(
                    "📊 Found {Count} follow-ups with due tips",
                    dueTips.Count());

                foreach (var followUp in dueTips)
                {
                    try
                    {
                        _logger.LogInformation(
                            "💡 Processing tip for user {UserId}, Severity: {Severity}, NextTipDate: {NextTipDate}",
                            followUp.UserId, followUp.SeverityLevel, followUp.NextTipDate);

                        // ⬅️ جيب tip عشوائي حسب الـ severity
                        var tip = await _tipRepo.GetRandomTipAsync(
                            followUp.UserId,
                            followUp.SeverityLevel);

                        if (tip != null)
                        {
                            // ⬅️ جيب لغة اليوزر
                            var user = await _userRepo.GetByIdAsync(followUp.UserId);
                            var userLang = user?.PreferredLanguage ?? "en";

                            _logger.LogInformation(
                                "🌐 User {UserId} language: {Language}",
                                followUp.UserId, userLang);

                            // ⬅️ اختار الـ Title والـ Body حسب اللغة
                            string title = LanguageHelper.GetLocalized(
                                "💚 نصيحة للصحة النفسية",
                                "💚 Mental Health Tip",
                                userLang);

                            string body = LanguageHelper.GetLocalized(
                                tip.TipTextAr,
                                tip.TipTextEnglish,
                                userLang);

                            _logger.LogInformation(
                                "📤 Sending tip {TipId} to user {UserId}: {Title}",
                                tip.TipId, followUp.UserId, title);

                            // ⬅️ بعت Notification
                            await _notificationService.SendRealtimeNotificationAsync(
                                followUp.UserId,
                                title,
                                body,
                                "MentalHealthTip",
                                tip.TipId,
                                actionUrl: $"/mental-health/tips/{tip.TipId}"
                            );

                            // ⬅️ سجّل إن الـ tip اتبعت
                            await _tipRepo.MarkTipAsSentAsync(followUp.UserId, tip.TipId);

                            _logger.LogInformation(
                                "✅ Tip {TipId} sent to user {UserId} in {Language}",
                                tip.TipId, followUp.UserId, userLang);
                        }
                        else
                        {
                            _logger.LogWarning(
                                "⚠️ No available tip found for user {UserId} with severity {Severity}",
                                followUp.UserId, followUp.SeverityLevel);
                        }

                        // ⬅️⬅️⬅️ المهم: حدد موعد الـ Tip الجاي حسب الـ Severity ⬅️⬅️⬅️
                        var (_, tipInterval) = GetIntervals(followUp.SeverityLevel);
                        followUp.NextTipDate = DateTime.Now.AddHours(1).Add(tipInterval);

                        await _followUpRepo.UpdateAsync(followUp);

                        _logger.LogInformation(
                            "📅 Next tip for user {UserId} scheduled for: {NextTipDate} (Interval: {Interval})",
                            followUp.UserId, followUp.NextTipDate, tipInterval);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex,
                            "❌ Error sending tip to user {UserId}",
                            followUp.UserId);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error in SendDueTipsAsync");
            }
        }

        // ── Private Helpers ────────────────────────────────

        /// <summary>
        /// يحدد الفترات الزمنية بين التقييمات والنصائح حسب شدة الحالة
        /// </summary>
        private (TimeSpan assessmentInterval, TimeSpan tipInterval) GetIntervals(string severityLevel)
        {
            return severityLevel switch
            {
                "Severe" => (
                    TimeSpan.FromDays(3),   // Assessment every 3 days
                    TimeSpan.FromDays(1)    // Tip every 1 day 📅
                ),
                "Moderate" => (
                    TimeSpan.FromDays(7),   // Assessment every 7 days
                    TimeSpan.FromDays(3)    // Tip every 3 days 📅
                ),
                "Mild" => (
                    TimeSpan.FromDays(14),  // Assessment every 14 days
                    TimeSpan.FromDays(7)    // Tip every 7 days 📅
                ),
                _ => ( // Minimal/Normal
                    TimeSpan.FromDays(30),  // Assessment every 30 days
                    TimeSpan.FromDays(14)   // Tip every 14 days 📅
                )
            };
        }

        private string GetAssessmentReminderMessage(string severityLevel, string language)
        {
            if (language.StartsWith("ar"))
            {
                return severityLevel switch
                {
                    "Severe" => "حان وقت إعادة تقييم صحتك النفسية. المتابعة المنتظمة مهمة جداً. يرجى إكمال التقييم الآن.",
                    "Moderate" => "مضى أسبوع على آخر تقييم. نود التحقق من صحتك النفسية. يرجى إكمال التقييم.",
                    "Mild" => "حان وقت الفحص الدوري للصحة النفسية. ساعدينا في تتبع تقدمك بإكمال التقييم.",
                    _ => "نأمل أن تكوني بخير. حان وقت تقييمك الشهري للصحة النفسية."
                };
            }
            else
            {
                return severityLevel switch
                {
                    "Severe" => "It's time to re-assess your mental health. Regular monitoring is very important. Please complete the assessment now.",
                    "Moderate" => "A week has passed since your last assessment. We'd like to check on your mental health. Please complete the assessment.",
                    "Mild" => "It's time for your regular mental health check-up. Help us track your progress by completing the assessment.",
                    _ => "We hope you're doing well. It's time for your monthly mental health assessment."
                };
            }
        }
    }
}