using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using MomEase.core.DTOS.TrackingReminderDTO;
using MomEase.core.Entities;
using MomEase.core.Enums;
using MomEase.core.Interfaces;
using MomEase.infra.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MomEase.infra.Services
{
    public class DailyTrackingReminderService : IDailyTrackingReminderService
    {
        private readonly IDailyTrackingReminderRepository _reminderRepository;
        private readonly IUserRepository _userRepository;
        private readonly IChildRepository _childRepository;
        private readonly IFeedingRecordRepository _feedingRecordRepository;
        private readonly ISleepRecordRepository _sleepRecordRepository;
        private readonly IGrowthRecordRepository _growthRecordRepository;
        private readonly INotificationService _notificationService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<DailyTrackingReminderService> _logger;

        public DailyTrackingReminderService(
            IDailyTrackingReminderRepository reminderRepository,
            IUserRepository userRepository,
            IChildRepository childRepository,
            IFeedingRecordRepository feedingRecordRepository,
            ISleepRecordRepository sleepRecordRepository,
            IGrowthRecordRepository growthRecordRepository,
            INotificationService notificationService,
            IHttpContextAccessor httpContextAccessor,
            ILogger<DailyTrackingReminderService> logger)
        {
            _reminderRepository = reminderRepository ?? throw new ArgumentNullException(nameof(reminderRepository));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _childRepository = childRepository ?? throw new ArgumentNullException(nameof(childRepository));
            _feedingRecordRepository = feedingRecordRepository ?? throw new ArgumentNullException(nameof(feedingRecordRepository));
            _sleepRecordRepository = sleepRecordRepository ?? throw new ArgumentNullException(nameof(sleepRecordRepository));
            _growthRecordRepository = growthRecordRepository ?? throw new ArgumentNullException(nameof(growthRecordRepository));
            _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        /// <summary>
        /// Send daily tracking reminders for all mothers
        /// </summary>
        public async Task SendDailyTrackingRemindersAsync()
        {
            try
            {
                _logger.LogInformation("📊 Starting daily tracking reminders...");

                // جيب كل الأمهات
                var allUsers = await _userRepository.GetAllAsync();
                var mothers = allUsers.Where(u => u.Role == Role.MOTHER).ToList();

                foreach (var mother in mothers)
                {
                    try
                    {
                        // ✅ تحقق إذا كانت اتبعت النهارده قبل كده
                        var todayReminders = await _reminderRepository
                            .GetByUserIdAndDateAsync(mother.UserId, DateTime.Now.AddHours(1));

                        if (todayReminders.Any())
                        {
                            _logger.LogInformation(
                                "⏭️ Reminder already sent today for user {UserId}",
                                mother.UserId);
                            continue; // ← skip
                        }
                        // جيب أطفال الأم
                        var children = await _childRepository.GetByUserIdAsync(mother.UserId);

                        if (!children.Any())
                            continue;

                        // لكل طفل - جيب الـ Tracking Status
                        var childrenStatus = new List<ChildTrackingStatusDto>();
                        var lang = mother.PreferredLanguage ?? "en";
                        foreach (var child in children)
                        {
                            var status = await GetChildTrackingStatusAsync(child, lang);
                            childrenStatus.Add(status);
                        }

                        // ابني الـ Notification الشامل
                        await SendTrackingNotificationAsync(mother, childrenStatus);

                        // احفظ في Database
                        var reminder = new DailyTrackingReminder
                        {
                            UserId = mother.UserId,
                            ChildId = children.FirstOrDefault()?.ChildId ?? 0,  // Reference child
                            ReminderDate = DateTime.Now.AddHours(1),
                            IsSent = true,
                            SentAt = DateTime.Now.AddHours(1)
                        };

                        await _reminderRepository.CreateAsync(reminder);

                        _logger.LogInformation(
                            "✅ Daily tracking reminder sent to user {UserId}",
                            mother.UserId);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex,
                            "❌ Error sending daily tracking reminder to user {UserId}",
                            mother.UserId);
                    }
                }

                _logger.LogInformation("📊 Daily tracking reminders completed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error in SendDailyTrackingRemindersAsync");
                throw;
            }
        }

        /// <summary>
        /// Get tracking status for specific child
        /// </summary>
        public async Task<DailyTrackingReminderDto?> GetTrackingStatusAsync(int userId)
        {
            try
            {
                var lang = LanguageHelper.GetLang(_httpContextAccessor);

                var children = await _childRepository.GetByUserIdAsync(userId);
                if (children == null || !children.Any())
                    return null;

                var childrenStatus = new List<ChildTrackingStatusDto>();
                foreach (var child in children)
                {
                    var status = await GetChildTrackingStatusAsync(child, lang);
                    childrenStatus.Add(status);
                }

                // ✅ ما تحفظيش في Database هنا!
                return new DailyTrackingReminderDto
                {
                    // ReminderId = 0  // ما تبعثيش معرف وهمي
                    UserId = userId,
                    ReminderDate = DateTime.Now.AddHours(1),
                    ChildrenStatus = childrenStatus,
                    Title = lang == "ar" ? "📊 تذكير بيانات اليوم" : "📊 Today's Tracking Reminder",
                    Message = GenerateSummaryMessage(childrenStatus, lang),
                    IsSent = false,  // ✅ صح - لأن ما احفظتش في DB
                    CreatedAt = DateTime.Now.AddHours(1),
                    ActionUrl = "/tracking/child/{childId}"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting tracking status for user {UserId}", userId);
                return null;
            }
        }

        #region Private Helpers

        /// <summary>
        /// Get tracking status for a specific child
        /// </summary>
        private async Task<ChildTrackingStatusDto> GetChildTrackingStatusAsync(Child child, string lang = "en")
        {
            var today = DateTime.Now.AddHours(1).Date;

            // جيب آخر feeding record
            var feedingStatus = await GetFeedingStatusAsync(child.ChildId, today, lang);

            // جيب آخر sleep record
            var sleepStatus = await GetSleepStatusAsync(child.ChildId, today, lang);

            // جيب آخر growth record
            var growthStatus = await GetGrowthStatusAsync(child.ChildId, lang);

            var missingCount = 0;
            if (!feedingStatus.HasData) missingCount++;
            if (!sleepStatus.HasData) missingCount++;
            if (!growthStatus.HasData) missingCount++;

            return new ChildTrackingStatusDto
            {
                ChildId = child.ChildId,
                ChildName = child.FullName,
                FeedingStatus = feedingStatus,
                SleepStatus = sleepStatus,
                GrowthStatus = growthStatus,
                TotalMissingDataCount = missingCount
            };
        }

        /// <summary>
        /// Get feeding status
        /// </summary>
        private async Task<FeedingStatusDto> GetFeedingStatusAsync(int childId, DateTime today, string lang = "en")
        {
            //var lang = LanguageHelper.GetLang(_httpContextAccessor);
            var records = await _feedingRecordRepository.GetByChildIdAsync(childId);
            var lastRecord = records.OrderByDescending(r => r.FeedingDate).FirstOrDefault();

            if (lastRecord == null)
                return new FeedingStatusDto
                {
                    Status = "❌",
                    Message = lang == "ar" ? "لا توجد بيانات تغذية" : "No feeding data found",
                    HasData = false
                };

            var daysSince = (today - lastRecord.FeedingDate.Date).Days;

            return new FeedingStatusDto
            {
                LastRecordId = lastRecord.RecordId,
                LastRecordDate = lastRecord.FeedingDate,
                TimesPerDay = lastRecord.FeedingTimesPerDay,
                Status = daysSince == 0 ? "✅" : "⚠️",
                Message = daysSince == 0
                    ? (lang == "ar" ? "محدّثة اليوم" : "Updated today")
                    : (lang == "ar"
                        ? $"لم تضيفي بيانات التغذية منذ {daysSince} أيام"
                        : $"No feeding data for {daysSince} days"),
                HasData = true
            };
        }

        /// <summary>
        /// Get sleep status
        /// </summary>
        private async Task<SleepStatusDto> GetSleepStatusAsync(int childId, DateTime today, string lang = "en")
        {
            //var lang = LanguageHelper.GetLang(_httpContextAccessor);
            var records = await _sleepRecordRepository.GetChildSleepRecordsAsync(childId);
            var lastRecord = records.OrderByDescending(r => r.SleepDate).FirstOrDefault();

            if (lastRecord == null)
                return new SleepStatusDto
                {
                    Status = "❌",
                    Message = lang == "ar" ? "لا توجد بيانات نوم" : "No sleep data found",
                    HasData = false
                };

            var daysSince = (today - lastRecord.SleepDate.Date).Days;

            return new SleepStatusDto
            {
                LastRecordId = lastRecord.RecordId,
                LastRecordDate = lastRecord.SleepDate,
                DurationFormatted = FormatTimeSpan(lastRecord.SleepDuration),
                Status = daysSince == 0 ? "✅" : "⚠️",
                Message = daysSince == 0
                    ? (lang == "ar" ? "محدّثة اليوم" : "Updated today")
                    : (lang == "ar"
                        ? $"لم تضيفي بيانات النوم منذ {daysSince} أيام"
                        : $"No sleep data for {daysSince} days"),
                HasData = true
            };
        }

        /// <summary>
        /// Get growth status
        /// </summary>
        private async Task<GrowthStatusDto> GetGrowthStatusAsync(int childId, string lang = "en")
        {
            //var lang = LanguageHelper.GetLang(_httpContextAccessor);
            var records = await _growthRecordRepository.GetByChildIdAsync(childId);
            var lastRecord = records.OrderByDescending(r => r.RecordDate).FirstOrDefault();

            if (lastRecord == null)
                return new GrowthStatusDto
                {
                    Status = "❌",
                    DaysSinceLastRecord = 999,
                    Message = lang == "ar" ? "لا توجد قياسات" : "No growth data found",
                    HasData = false
                };

            var daysSince = (DateTime.Now.AddHours(1).Date - lastRecord.RecordDate.Date).Days;

            return new GrowthStatusDto
            {
                LastRecordId = lastRecord.GrowthId,
                LastRecordDate = lastRecord.RecordDate,
                Weight = (double?)lastRecord.WeightKg,
                Height = (double?)lastRecord.HeightCm,
                DaysSinceLastRecord = daysSince,
                Status = daysSince <= 7 ? "✅" : "⚠️",
                Message = daysSince <= 7
                    ? (lang == "ar" ? "محدّثة" : "Updated")
                    : (lang == "ar"
                        ? $"لم تضيفي قياساً منذ {daysSince} أيام"
                        : $"No growth measurement for {daysSince} days"),
                HasData = true
            };
        }

        /// <summary>
        /// Send notification to mother
        /// </summary>
        private async Task SendTrackingNotificationAsync(Users mother, List<ChildTrackingStatusDto> childrenStatus)
        {
            var lang = mother.PreferredLanguage ?? "en";
            var title = lang == "ar" ? "📊 تذكير بيانات اليوم" : "📊 Daily Tracking Reminder";
            var message = GenerateSummaryMessage(childrenStatus, lang);

            int firstChildId = childrenStatus.FirstOrDefault()?.ChildId ?? 0;

            await _notificationService.SendRealtimeNotificationAsync(
                    mother.UserId,
                    title,
                    message,   // ← بس كده - النص البسيط
                    "DailyTrackingReminder",
                    firstChildId,
                    "/tracking"
             );
        }

        /// <summary>
        /// Generate summary message
        /// </summary>
        private string GenerateSummaryMessage(List<ChildTrackingStatusDto> childrenStatus, string lang = "en")
        {
            var totalMissing = childrenStatus.Sum(c => c.TotalMissingDataCount);

            if (lang == "ar")
            {
                var missingItems = new List<string>();

                // ❌ مفيش بيانات خالص
                if (childrenStatus.Any(c => !c.FeedingStatus.HasData))
                    missingItems.Add("التغذية");
                // ⚠️ عندها بيانات بس مش النهارده
                else if (childrenStatus.Any(c => c.FeedingStatus.Status == "⚠️"))
                    missingItems.Add("التغذية");

                if (childrenStatus.Any(c => !c.SleepStatus.HasData))
                    missingItems.Add("النوم");
                else if (childrenStatus.Any(c => c.SleepStatus.Status == "⚠️"))
                    missingItems.Add("النوم");

                if (childrenStatus.Any(c => !c.GrowthStatus.HasData))
                    missingItems.Add("القياسات");
                else if (childrenStatus.Any(c => c.GrowthStatus.Status == "⚠️"))
                    missingItems.Add("القياسات");

                if (!missingItems.Any())
                    return "✅ شكراً! جميع البيانات محدّثة لجميع الأطفال";

                var childrenNames = childrenStatus
            .Where(c => !c.FeedingStatus.HasData || c.FeedingStatus.Status == "⚠️"
                     || !c.SleepStatus.HasData || c.SleepStatus.Status == "⚠️"
                     || !c.GrowthStatus.HasData || c.GrowthStatus.Status == "⚠️")
            .Select(c => c.ChildName);

                return $"⚠️ {string.Join("، ", childrenNames)}: الرجاء إضافة {string.Join("، ", missingItems)}";

            }
            else
            {
                var missingItems = new List<string>();

                if (childrenStatus.Any(c => !c.FeedingStatus.HasData))
                    missingItems.Add("Feeding");
                else if (childrenStatus.Any(c => c.FeedingStatus.Status == "⚠️"))
                    missingItems.Add("Feeding");

                if (childrenStatus.Any(c => !c.SleepStatus.HasData))
                    missingItems.Add("Sleep");
                else if (childrenStatus.Any(c => c.SleepStatus.Status == "⚠️"))
                    missingItems.Add("Sleep");

                if (childrenStatus.Any(c => !c.GrowthStatus.HasData))
                    missingItems.Add("Growth");
                else if (childrenStatus.Any(c => c.GrowthStatus.Status == "⚠️"))
                    missingItems.Add("Growth");

                if (!missingItems.Any())
                    return "✅ Great! All data is up to date for all children";

                var childrenNames = childrenStatus
             .Where(c => !c.FeedingStatus.HasData || c.FeedingStatus.Status == "⚠️"
                      || !c.SleepStatus.HasData || c.SleepStatus.Status == "⚠️"
                      || !c.GrowthStatus.HasData || c.GrowthStatus.Status == "⚠️")
             .Select(c => c.ChildName);

                return $"⚠️ {string.Join(", ", childrenNames)}: please add {string.Join(", ", missingItems)}";

            }
        }

        private string FormatTimeSpan(TimeSpan timeSpan)
        {
            return $"{(int)timeSpan.TotalHours}h {timeSpan.Minutes}m";
        }

        #endregion
    }
}
