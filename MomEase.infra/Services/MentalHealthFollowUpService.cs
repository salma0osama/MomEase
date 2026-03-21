using Microsoft.Extensions.Logging;
using MomEase.core.Entities;
using MomEase.core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

                // حدد المدد حسب شدة الاكتئاب
                var (assessmentInterval, tipInterval) = GetIntervals(severityLevel);

                var followUp = new MentalHealthFollowUp
                {
                    UserId = userId,
                    LastAssessmentResultId = assessmentResultId,
                    SeverityLevel = severityLevel,
                    NextAssessmentDate = DateTime.Now.Add(assessmentInterval),
                    NextTipDate = DateTime.Now.Add(tipInterval),
                    AssessmentReminderSent = false,
                    IsCompleted = false,
                    CreatedAt = DateTime.Now
                };

                await _followUpRepo.CreateAsync(followUp);

                _logger.LogInformation(
                    "✅ Follow-up plan created. Next assessment: {NextAssessment}, Next tip: {NextTip}",
                    followUp.NextAssessmentDate, followUp.NextTipDate);
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

                        // ⬅️ جيب الـ User عشان تعرف لغته
                        var user = await _userRepo.GetByIdAsync(followUp.UserId);
                        var userLang = user?.PreferredLanguage ?? "en";

                        // اختار الـ Title حسب اللغة
                        var title = userLang.StartsWith("ar")
                            ? "🧠 وقت تقييم صحتك النفسية"
                            : "🧠 Mental Health Check-in Time";

                        // اختار الـ Message حسب اللغة
                        var message = GetAssessmentReminderMessage(followUp.SeverityLevel, userLang);

                        await _notificationService.SendRealtimeNotificationAsync(
                            followUp.UserId,
                            title,
                            message,
                            "MentalHealthAssessmentReminder",
                            followUp.LastAssessmentResultId
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

                foreach (var followUp in dueTips)
                {
                    try
                    {
                        _logger.LogInformation(
                            "💡 Sending mental health tip to user {UserId}",
                            followUp.UserId);

                        var tip = await _tipRepo.GetRandomTipAsync(
                            followUp.UserId,
                            followUp.SeverityLevel);

                        if (tip != null)
                        {
                            // ⬅️ جيب الـ User عشان تعرف لغته
                            var user = await _userRepo.GetByIdAsync(followUp.UserId);
                            var userLang = user?.PreferredLanguage ?? "en";

                            // اختار الـ Tip Text حسب اللغة
                            var tipText = userLang.StartsWith("ar") && !string.IsNullOrEmpty(tip.TipTextAr)
                                ? tip.TipTextAr
                                : tip.TipTextEnglish;

                            // اختار الـ Title حسب اللغة
                            var title = userLang.StartsWith("ar")
                                ? "💚 نصيحة للصحة النفسية"
                                : "💚 Mental Health Tip";

                            await _notificationService.SendRealtimeNotificationAsync(
                                followUp.UserId,
                                title,
                                tipText,
                                "MentalHealthTip",
                                tip.TipId
                            );

                            await _tipRepo.MarkTipAsSentAsync(followUp.UserId, tip.TipId);

                            _logger.LogInformation(
                                "✅ Tip {TipId} sent to user {UserId} in {Language}",
                                tip.TipId, followUp.UserId, userLang);
                        }

                        var (_, tipInterval) = GetIntervals(followUp.SeverityLevel);
                        followUp.NextTipDate = DateTime.Now.Add(tipInterval);
                        await _followUpRepo.UpdateAsync(followUp);
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

        private (TimeSpan assessmentInterval, TimeSpan tipInterval) GetIntervals(string severityLevel)
        {
            return severityLevel switch
            {
                "Severe" => (TimeSpan.FromDays(3), TimeSpan.FromDays(1)),      // Every 3 days assessment, daily tip
                "Moderate" => (TimeSpan.FromDays(7), TimeSpan.FromDays(3)),    // Weekly assessment, tip every 3 days
                "Mild" => (TimeSpan.FromDays(14), TimeSpan.FromDays(7)),       // Bi-weekly assessment, weekly tip
                _ => (TimeSpan.FromDays(30), TimeSpan.FromDays(14))            // Minimal: Monthly assessment, tip every 2 weeks
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
