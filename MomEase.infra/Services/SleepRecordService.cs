using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using MomEase.core.DTOS.SleepRecordDTO;
using MomEase.core.Entities;
using MomEase.core.Interfaces;
using MomEase.infra.Helpers;
using System.Globalization;

namespace MomEase.infra.Services
{
    public class SleepRecordService : ISleepRecordService
    {
        private readonly ISleepRecordRepository _sleepRecordRepository;
        private readonly IChildRepository _childRepository;
        private readonly ISleepReferenceRepository _sleepReferenceRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<SleepRecordService> _logger;

        public SleepRecordService(
            ISleepRecordRepository sleepRecordRepository,
            IChildRepository childRepository,
            ISleepReferenceRepository sleepReferenceRepository,
            IHttpContextAccessor httpContextAccessor,
            ILogger<SleepRecordService> logger)
        {
            _sleepRecordRepository = sleepRecordRepository;
            _childRepository = childRepository;
            _sleepReferenceRepository = sleepReferenceRepository;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public async Task<SleepRecordDto> CreateSleepRecordAsync(int userId, CreateSleepRecordDto dto)
        {
            try
            {
                // Verify child belongs to user
                var child = await _childRepository.GetChildByIdAsync(dto.ChildId);
                if (child == null)
                    throw new KeyNotFoundException("Child not found");

                if (child.UserId != userId)
                    throw new UnauthorizedAccessException("You are not authorized to add records for this child");

                // Validate date
                if (dto.SleepDate > DateTime.Now)
                    throw new ArgumentException("Sleep date cannot be in the future");

                // ✅ Parse times من string لـ TimeSpan
                if (!TimeSpan.TryParse(dto.SleepStartTime, out var startTime))
                    throw new ArgumentException("Invalid start time format. Use HH:mm (e.g., 20:30)");

                if (!TimeSpan.TryParse(dto.SleepEndTime, out var endTime))
                    throw new ArgumentException("Invalid end time format. Use HH:mm (e.g., 07:00)");

                // ✅ Remove the duplicate check - يقبل عدة sessions في نفس اليوم
                // (احذفي الأسطر اللي بتفحص ExistsForDateAsync)

                // ✅ Calculate duration
                TimeSpan duration;
                if (endTime < startTime)
                {
                    // Overnight sleep (8 PM to 7 AM)
                    duration = (TimeSpan.FromHours(24) - startTime) + endTime;
                }
                else
                {
                    duration = endTime - startTime;
                }

                // Validate reasonable sleep duration
                if (duration.TotalHours < 0.25 || duration.TotalHours > 12)
                {
                    throw new ArgumentException(
                        $"Sleep duration ({duration.TotalHours:F1} hours) is not realistic. " +
                        "Please verify the start and end times.");
                }

                // Get reference للمقارنة
                var ageInMonths = CalculateAgeInMonths(child.BirthDate);
                var reference = await _sleepReferenceRepository.GetByAgeAsync(ageInMonths);

                // ✅ Create record
                var record = new ChildSleepRecord
                {
                    ChildId = dto.ChildId,
                    SleepDate = dto.SleepDate,
                    SleepStartTime = startTime,     // ✅ NEW
                    SleepEndTime = endTime,         // ✅ NEW
                    Quality = dto.Quality,          // ✅ NEW
                    Notes = dto.Notes,
                    SleepRefId = reference?.SleepRefId,
                    CreatedAt = DateTime.Now
                };

                var createdRecord = await _sleepRecordRepository.AddSleepRecordAsync(record);

                _logger.LogInformation(
                    "Sleep session created {RecordId} for child {ChildId}: {StartTime} to {EndTime} ({Duration} hours)",
                    createdRecord.RecordId, dto.ChildId,
                    createdRecord.SleepStartTime, createdRecord.SleepEndTime,
                    createdRecord.SleepDuration.TotalHours);

                return MapToDto(createdRecord, reference);
            }
            catch (Exception ex) when (ex is KeyNotFoundException || ex is ArgumentException ||
                                       ex is UnauthorizedAccessException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create sleep record");
                throw new InvalidOperationException($"Failed to create sleep record: {ex.Message}", ex);
            }
        }

        public async Task<List<SleepRecordDto>> GetChildSleepRecordsAsync(int childId, int userId)
        {
            try
            {
                var child = await _childRepository.GetChildByIdAsync(childId);
                if (child == null)
                    throw new KeyNotFoundException("Child not found");

                if (child.UserId != userId)
                    throw new UnauthorizedAccessException("You are not authorized to access this child's records");

                var records = await _sleepRecordRepository.GetChildSleepRecordsAsync(childId);

                // جلب الـ Reference مرة واحدة
                var ageInMonths = CalculateAgeInMonths(child.BirthDate);
                var reference = await _sleepReferenceRepository.GetByAgeAsync(ageInMonths);

                return records.Select(r => MapToDto(r, reference)).ToList();
            }
            catch (Exception ex) when (ex is KeyNotFoundException || ex is UnauthorizedAccessException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve sleep records");
                throw new InvalidOperationException($"Failed to retrieve sleep records: {ex.Message}", ex);
            }
        }

        public async Task<SleepRecordDto> GetSleepRecordByIdAsync(int recordId, int userId)
        {
            try
            {
                var record = await _sleepRecordRepository.GetSleepRecordByIdAsync(recordId);
                if (record == null)
                    throw new KeyNotFoundException("Sleep record not found");

                if (record.Child.UserId != userId)
                    throw new UnauthorizedAccessException("You are not authorized to access this record");

                // جلب الـ Reference
                var ageInMonths = CalculateAgeInMonths(record.Child.BirthDate);
                var reference = await _sleepReferenceRepository.GetByAgeAsync(ageInMonths);

                return MapToDto(record, reference);
            }
            catch (Exception ex) when (ex is KeyNotFoundException || ex is UnauthorizedAccessException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve sleep record");
                throw new InvalidOperationException($"Failed to retrieve sleep record: {ex.Message}", ex);
            }
        }

        public async Task<SleepRecordDto> UpdateSleepRecordAsync(int recordId, int userId, UpdateSleepRecordDto dto)
        {
            try
            {
                var record = await _sleepRecordRepository.GetSleepRecordByIdAsync(recordId);
                if (record == null)
                    throw new KeyNotFoundException("Sleep record not found");

                if (record.Child.UserId != userId)
                    throw new UnauthorizedAccessException("You are not authorized to update this record");

                // ✅ Remove duplicate check - يقبل عدة sessions في نفس اليوم
                // (احذفي الأسطر اللي بتفحص ExistsForDateAsync)

                if (dto.SleepDate.HasValue)
                {
                    if (dto.SleepDate.Value > DateTime.Now)
                        throw new ArgumentException("Sleep date cannot be in the future");
                    record.SleepDate = dto.SleepDate.Value;
                }

                // ✅ Update start time if provided
                if (!string.IsNullOrEmpty(value: dto.SleepStartTime))
                {
                    if (!TimeSpan.TryParse(dto.SleepStartTime, out var startTime))
                        throw new ArgumentException("Invalid start time format. Use HH:mm (e.g., 20:30)");
                    record.SleepStartTime = startTime;
                }

                // ✅ Update end time if provided
                if (!string.IsNullOrEmpty(dto.SleepEndTime))
                {
                    if (!TimeSpan.TryParse(dto.SleepEndTime, out var endTime))
                        throw new ArgumentException("Invalid end time format. Use HH:mm (e.g., 07:00)");
                    record.SleepEndTime = endTime;
                }

                // ✅ Remove old code
                // if (!string.IsNullOrEmpty(dto.SleepHoursTotal))
                // {
                //     if (!TimeSpan.TryParse(dto.SleepHoursTotal, out var parsed))
                //         throw new ArgumentException("Invalid sleep hours format. Use HH:mm:ss (e.g., 08:30:00)");
                //     record.SleepHoursTotal = parsed;
                // }

                if (dto.SleepRefId.HasValue)
                    record.SleepRefId = dto.SleepRefId;

                // ✅ Update quality if provided
                if (!string.IsNullOrEmpty(dto.Quality))
                    record.Quality = dto.Quality;

                if (dto.Notes != null)
                    record.Notes = dto.Notes;

                var updatedRecord = await _sleepRecordRepository.UpdateSleepRecordAsync(record);
                _logger.LogInformation("Sleep record updated {RecordId}", recordId);

                var ageInMonths = CalculateAgeInMonths(record.Child.BirthDate);
                var reference = await _sleepReferenceRepository.GetByAgeAsync(ageInMonths);

                return MapToDto(updatedRecord, reference);
            }
            catch (Exception ex) when (ex is KeyNotFoundException || ex is ArgumentException ||
                                       ex is InvalidOperationException || ex is UnauthorizedAccessException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update sleep record");
                throw new InvalidOperationException($"Failed to update sleep record: {ex.Message}", ex);
            }
        }

        public async Task<bool> DeleteSleepRecordAsync(int recordId, int userId)
        {
            try
            {
                var record = await _sleepRecordRepository.GetSleepRecordByIdAsync(recordId);
                if (record == null)
                    throw new KeyNotFoundException("Sleep record not found");

                if (record.Child.UserId != userId)
                    throw new UnauthorizedAccessException("You are not authorized to delete this record");

                await _sleepRecordRepository.DeleteSleepRecordAsync(record);
                _logger.LogInformation("Sleep record deleted {RecordId}", recordId);

                return true;
            }
            catch (Exception ex) when (ex is KeyNotFoundException || ex is UnauthorizedAccessException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete sleep record");
                throw new InvalidOperationException($"Failed to delete sleep record: {ex.Message}", ex);
            }
        }

        public async Task<SleepStatisticsDto> GetSleepStatisticsAsync(int childId, int userId)
        {
            try
            {
                var lang = LanguageHelper.GetLang(_httpContextAccessor);

                var child = await _childRepository.GetChildByIdAsync(childId);
                if (child == null)
                    throw new KeyNotFoundException("Child not found");

                if (child.UserId != userId)
                    throw new UnauthorizedAccessException("You are not authorized to access this child's statistics");

                var records = await _sleepRecordRepository.GetChildSleepRecordsAsync(childId);
                // ✅ Remove: var recordsWithSleep = records.Where(r => r.SleepHoursTotal.HasValue).ToList();

                // ✅ Use instead:
                var recordsWithSleep = records.Where(r => r.SleepDuration.Ticks > 0).ToList();

                var ageInMonths = CalculateAgeInMonths(child.BirthDate);
                var reference = await _sleepReferenceRepository.GetByAgeAsync(ageInMonths);

                if (!recordsWithSleep.Any())
                {
                    return new SleepStatisticsDto
                    {
                        TotalRecords = 0,
                        AverageSleepHours = TimeSpan.Zero,
                        AverageSleepHoursFormatted = "0h 0m",
                        MaxSleepHours = TimeSpan.Zero,
                        MinSleepHours = TimeSpan.Zero,
                        GoodSleepDays = 0,
                        NormalSleepDays = 0,
                        PoorSleepDays = 0,
                        SleepQualityPercentage = 0,
                        Last7DaysAverage = TimeSpan.Zero,
                        Last7DaysAverageFormatted = "0h 0m",
                        CurrentSleepStatus = lang == "ar" ? "لا توجد بيانات" : "No Data",
                        MostCommonStatus = lang == "ar" ? "لا توجد بيانات" : "No Data",
                        ComparisonWithReference = new ComparisonWithSleepReferenceDto
                        {
                            Status = lang == "ar" ? "لا توجد بيانات" : "No Data",
                            RecommendedMinHours = reference?.SleepMinHours?.TotalHours ?? 0,
                            RecommendedMaxHours = reference?.SleepMaxHours?.TotalHours ?? 0,
                            ActualAverageHours = 0,
                            Message = lang == "ar"
                                ? "لا توجد سجلات نوم متاحة حتى الآن"
                                : "No sleep records available yet"
                        }
                    };
                }

                // ✅ Calculate average using SleepDuration
                var avgTicks = (long)recordsWithSleep.Average(r => r.SleepDuration.Ticks);
                var avgSleep = new TimeSpan(avgTicks);

                // ✅ Last 7 days average
                var last7Days = await _sleepRecordRepository.GetLastNDaysAsync(childId, 7);
                var last7DaysWithSleep = last7Days.Where(r => r.SleepDuration.Ticks > 0).ToList();

                TimeSpan last7DaysAvgTimeSpan = TimeSpan.Zero;
                double last7DaysAverage = 0;

                if (last7DaysWithSleep.Any())
                {
                    var last7AvgTicks = (long)last7DaysWithSleep.Average(r => r.SleepDuration.Ticks);
                    last7DaysAvgTimeSpan = new TimeSpan(last7AvgTicks);
                    last7DaysAverage = last7DaysWithSleep.Average(r => r.SleepDuration.TotalHours);
                }

                // ✅ Calculate Good, Normal, Poor Days
                var goodSleep = recordsWithSleep.Count(r =>
                    reference != null &&
                    reference.SleepMinHours.HasValue &&
                    reference.SleepMaxHours.HasValue &&
                    r.SleepDuration.TotalHours >= reference.SleepMinHours.Value.TotalHours &&
                    r.SleepDuration.TotalHours <= reference.SleepMaxHours.Value.TotalHours
                );

                var poorSleep = recordsWithSleep.Count(r =>
                    reference != null &&
                    reference.SleepMinHours.HasValue &&
                    r.SleepDuration.TotalHours < reference.SleepMinHours.Value.TotalHours
                );

                var normalSleep = recordsWithSleep.Count(r =>
                    reference != null &&
                    reference.SleepMaxHours.HasValue &&
                    r.SleepDuration.TotalHours > reference.SleepMaxHours.Value.TotalHours
                );

                var statusCounts = new Dictionary<string, int>
                {
                    { lang == "ar" ? "جيد" : "Good", goodSleep },
                    { lang == "ar" ? "سيء" : "Poor", poorSleep },
                    { lang == "ar" ? "عادي" : "Normal", normalSleep }
                };
                var mostCommonStatus = statusCounts.OrderByDescending(kvp => kvp.Value).FirstOrDefault().Key ?? (lang == "ar" ? "غير معروف" : "Unknown");

                var currentStatus = GetSleepStatusCategory(last7DaysAverage, reference, lang);

                var comparison = new ComparisonWithSleepReferenceDto
                {
                    Status = currentStatus,
                    RecommendedMinHours = reference?.SleepMinHours?.TotalHours ?? 0,
                    RecommendedMaxHours = reference?.SleepMaxHours?.TotalHours ?? 0,
                    ActualAverageHours = Math.Round(last7DaysAverage, 1),
                    Message = GenerateComparisonMessage(last7DaysAverage, reference, lang)
                };

                return new SleepStatisticsDto
                {
                    TotalRecords = recordsWithSleep.Count,
                    AverageSleepHours = avgSleep,
                    AverageSleepHoursFormatted = FormatTimeSpan(avgSleep),
                    MaxSleepHours = recordsWithSleep.Max(r => r.SleepDuration),
                    MinSleepHours = recordsWithSleep.Min(r => r.SleepDuration),
                    GoodSleepDays = goodSleep,
                    NormalSleepDays = normalSleep,
                    PoorSleepDays = poorSleep,
                    SleepQualityPercentage = recordsWithSleep.Count > 0
                        ? Math.Round((double)goodSleep / recordsWithSleep.Count * 100, 1)
                        : 0,
                    Last7DaysAverage = last7DaysAvgTimeSpan,
                    Last7DaysAverageFormatted = FormatTimeSpan(last7DaysAvgTimeSpan),
                    CurrentSleepStatus = currentStatus,
                    MostCommonStatus = mostCommonStatus,
                    ComparisonWithReference = comparison
                };
            }
            catch (Exception ex) when (ex is KeyNotFoundException || ex is UnauthorizedAccessException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve sleep statistics");
                throw new InvalidOperationException($"Failed to retrieve sleep statistics: {ex.Message}", ex);
            }
        }

        public async Task<WeeklySleepDto> GetWeeklySleepAsync(int childId, int userId)
        {
            try
            {
                var child = await _childRepository.GetChildByIdAsync(childId);
                if (child == null)
                    throw new KeyNotFoundException("Child not found");

                if (child.UserId != userId)
                    throw new UnauthorizedAccessException("You are not authorized to access this child's records");

                var today = DateTime.Now.Date;
                var weekStart = today.AddDays(-6);  // آخر 7 أيام
                var weekEnd = today;

                var records = await _sleepRecordRepository.GetSleepRecordsByDateRangeAsync(childId, weekStart, weekEnd);

                var ageInMonths = CalculateAgeInMonths(child.BirthDate);
                var reference = await _sleepReferenceRepository.GetByAgeAsync(ageInMonths);

                // ✅ Group by date and sum total sleep
                var dailySleep = Enumerable.Range(0, 7).Select(i =>
                {
                    var date = weekStart.AddDays(i);
                    var daySessions = records.Where(r => r.SleepDate.Date == date).ToList();

                    // ✅ Calculate total sleep for the day (مجموع جميع الجلسات)
                    var totalSleepTicks = daySessions.Sum(s => s.SleepDuration.Ticks);
                    var totalSleep = totalSleepTicks > 0 ? new TimeSpan(totalSleepTicks) : (TimeSpan?)null;

                    return new DailySleepDto
                    {
                        Date = date,
                        SleepHours = totalSleep,
                        SleepHoursFormatted = FormatTimeSpan(totalSleep),
                        Status = GetSleepStatus(totalSleep, reference),
                        SessionCount = daySessions.Count  // عدد الجلسات في اليوم
                    };
                }).ToList();

                // ✅ Calculate weekly average
                var recordsWithSleep = records.Where(r => r.SleepDuration.Ticks > 0).ToList();
                var avgTicks = recordsWithSleep.Any()
                    ? (long)recordsWithSleep.Average(r => r.SleepDuration.Ticks)
                    : 0;

                return new WeeklySleepDto
                {
                    WeekStart = weekStart,
                    WeekEnd = weekEnd,
                    DailySleep = dailySleep,
                    WeeklyAverageSleep = new TimeSpan(avgTicks),
                    WeeklyAverageSleepFormatted = FormatTimeSpan(new TimeSpan(avgTicks)),
                    TotalRecords = recordsWithSleep.Count,
                    TotalSessions = records.Count
                };
            }
            catch (Exception ex) when (ex is KeyNotFoundException || ex is UnauthorizedAccessException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve weekly sleep data");
                throw new InvalidOperationException($"Failed to retrieve weekly sleep data: {ex.Message}", ex);
            }
        }

        public async Task<MonthlySleepDto> GetMonthlySleepAsync(int childId, int userId)
        {
            try
            {
                var child = await _childRepository.GetChildByIdAsync(childId);
                if (child == null)
                    throw new KeyNotFoundException("Child not found");

                if (child.UserId != userId)
                    throw new UnauthorizedAccessException("You are not authorized to access this child's records");

                var today = DateTime.Now.Date;
                var monthStart = new DateTime(today.Year, today.Month, 1);
                var monthEnd = today;  // ✅ Changed: لحد النهاردة بس مش آخر الشهر

                var records = await _sleepRecordRepository.GetSleepRecordsByDateRangeAsync(childId, monthStart, monthEnd);

                var ageInMonths = CalculateAgeInMonths(child.BirthDate);
                var reference = await _sleepReferenceRepository.GetByAgeAsync(ageInMonths);

                // ✅ Group by date and sum
                var daysInRange = (monthEnd - monthStart).Days + 1;
                var dailySleep = Enumerable.Range(0, daysInRange).Select(day =>
                {
                    var date = monthStart.AddDays(day);
                    var daySessions = records.Where(r => r.SleepDate.Date == date).ToList();

                    // ✅ Sum all sessions for the day
                    var totalSleepTicks = daySessions.Sum(s => s.SleepDuration.Ticks);
                    var totalSleep = totalSleepTicks > 0 ? new TimeSpan(totalSleepTicks) : (TimeSpan?)null;

                    return new DailySleepDto
                    {
                        Date = date,
                        SleepHours = totalSleep,
                        SleepHoursFormatted = FormatTimeSpan(totalSleep),
                        Status = GetSleepStatus(totalSleep, reference),
                        SessionCount = daySessions.Count
                    };
                }).ToList();

                // ✅ Calculate average
                var recordsWithSleep = records.Where(r => r.SleepDuration.Ticks > 0).ToList();
                var avgTicks = recordsWithSleep.Any()
                    ? (long)recordsWithSleep.Average(r => r.SleepDuration.Ticks)
                    : 0;

                var goodDays = recordsWithSleep.Count(r =>
                    reference != null &&
                    reference.SleepMinHours.HasValue &&
                    reference.SleepMaxHours.HasValue &&
                    r.SleepDuration.TotalHours >= reference.SleepMinHours.Value.TotalHours &&
                    r.SleepDuration.TotalHours <= reference.SleepMaxHours.Value.TotalHours
                );

                var poorDays = recordsWithSleep.Count(r =>
                    reference != null &&
                    reference.SleepMinHours.HasValue &&
                    r.SleepDuration.TotalHours < reference.SleepMinHours.Value.TotalHours
                );

                return new MonthlySleepDto
                {
                    Year = today.Year,
                    Month = today.Month,
                    MonthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(today.Month),
                    DailySleep = dailySleep,
                    MonthlyAverageSleep = new TimeSpan(avgTicks),
                    MonthlyAverageSleepFormatted = FormatTimeSpan(new TimeSpan(avgTicks)),
                    TotalRecords = recordsWithSleep.Count,
                    GoodDays = goodDays,
                    PoorDays = poorDays
                };
            }
            catch (Exception ex) when (ex is KeyNotFoundException || ex is UnauthorizedAccessException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve monthly sleep data");
                throw new InvalidOperationException($"Failed to retrieve monthly sleep data: {ex.Message}", ex);
            }
        }

        #region Helper Methods

        private SleepRecordDto MapToDto(ChildSleepRecord record, SleepReference? reference = null)
        {
            return new SleepRecordDto
            {
                RecordId = record.RecordId,
                ChildId = record.ChildId,
                ChildName = record.Child?.FullName,
                SleepDate = record.SleepDate,

                // ✅ NEW: الأوقات
                SleepStartTime = record.SleepStartTime,
                SleepEndTime = record.SleepEndTime,
                SleepStartTimeFormatted = record.SleepStartTime.ToString(@"hh\:mm"),
                SleepEndTimeFormatted = record.SleepEndTime.ToString(@"hh\:mm"),

                // ✅ NEW: المدة المحسوبة
                SleepDuration = record.SleepDuration,
                SleepDurationFormatted = FormatTimeSpan(record.SleepDuration),

                Quality = record.Quality,  // ✅ NEW
                Notes = record.Notes,
                Status = GetSleepStatus(record.SleepDuration, reference),  // ✅ Changed

                ReferenceInfo = reference != null ? new SleepingReferenceInfo
                {
                    SleepMinHours = reference.SleepMinHours ?? TimeSpan.Zero,
                    SleepMaxHours = reference.SleepMaxHours ?? TimeSpan.Zero,
                    SleepMinHoursFormatted = FormatTimeSpan(reference.SleepMinHours),
                    SleepMaxHoursFormatted = FormatTimeSpan(reference.SleepMaxHours),
                    AgeRange = $"{reference.AgeMinMonths}-{reference.AgeMaxMonths} months"
                } : null
            };
        }

        private string FormatTimeSpan(TimeSpan? timeSpan)
        {
            if (!timeSpan.HasValue) return "N/A";
            return $"{(int)timeSpan.Value.TotalHours}h {timeSpan.Value.Minutes}m";
        }

        private string GetSleepStatus(TimeSpan? sleepHours, SleepReference? reference = null)
        {
            if (!sleepHours.HasValue) return "Unknown";

            if (reference != null && reference.SleepMinHours.HasValue && reference.SleepMaxHours.HasValue)
            {
                var hours = sleepHours.Value.TotalHours;
                var minHours = reference.SleepMinHours.Value.TotalHours;
                var maxHours = reference.SleepMaxHours.Value.TotalHours;

                if (hours >= minHours && hours <= maxHours)
                    return "Good";
                else if (hours < minHours)
                    return "Poor";
                else
                    return "Normal";
            }

            var hrs = sleepHours.Value.TotalHours;
            if (hrs >= 10) return "Good";
            if (hrs >= 8) return "Normal";
            return "Poor";
        }

        /// <summary>
        /// ✅ إضافة: Get status category for comparison
        /// </summary>
        private string GetSleepStatusCategory(double actualHours, SleepReference? reference, string lang = "en")
        {
            if (reference == null || !reference.SleepMinHours.HasValue || !reference.SleepMaxHours.HasValue)
                return lang == "ar" ? "لا توجد مرجعية" : "No Reference";

            var min = reference.SleepMinHours.Value.TotalHours;
            var max = reference.SleepMaxHours.Value.TotalHours;

            if (actualHours >= min && actualHours <= max)
                return lang == "ar" ? "جيد" : "Good";
            else if (actualHours < min)
                return lang == "ar" ? "سيء" : "Poor";
            else
                return lang == "ar" ? "عادي" : "Normal";
        }

        /// <summary>
        /// ✅ إضافة: Generate comparison message (مثل Feeding)
        /// </summary>
        private string GenerateComparisonMessage(double actualHours, SleepReference? reference, string lang = "en")
        {
            if (reference == null || !reference.SleepMinHours.HasValue || !reference.SleepMaxHours.HasValue)
                return lang == "ar"
                    ? "لا توجد بيانات مرجعية متاحة لهذه الفئة العمرية"
                    : "No reference data available for this age range";

            var min = reference.SleepMinHours.Value.TotalHours;
            var max = reference.SleepMaxHours.Value.TotalHours;

            if (lang == "ar")
            {
                // العربية
                if (actualHours < min * 0.7)
                    return $"⚠️ مدة النوم أقل بكثير من الموصى به ({min}-{max} ساعات/يوم). يرجى استشارة طبيب الأطفال.";

                if (actualHours < min)
                    return $"⚠️ مدة النوم أقل قليلاً من الموصى به ({min}-{max} ساعات/يوم).";

                if (actualHours >= min && actualHours <= max)
                    return $"✅ الطفل ينام ضمن النطاق الموصى به ({min}-{max} ساعات/يوم).";

                if (actualHours <= max * 1.2)
                    return $"مدة النوم أعلى قليلاً من الموصى به ({min}-{max} ساعات/يوم)، وهذا آمن بشكل عام.";

                return $"⚠️ مدة النوم أعلى بكثير من الموصى به ({min}-{max} ساعات/يوم). راقبي صحة الطفل بشكل عام.";
            }
            else
            {
                // الإنجليزية
                if (actualHours < min * 0.7)
                    return $"⚠️ Sleep duration is significantly below recommended ({min}-{max} hours/day). Please consult with a pediatrician.";

                if (actualHours < min)
                    return $"⚠️ Sleep duration is slightly below recommended ({min}-{max} hours/day).";

                if (actualHours >= min && actualHours <= max)
                    return $"✅ Your baby is sleeping within the recommended range ({min}-{max} hours/day).";

                if (actualHours <= max * 1.2)
                    return $"Sleep duration is slightly above recommended ({min}-{max} hours/day), which is generally fine.";

                return $"⚠️ Sleep duration is significantly above recommended ({min}-{max} hours/day). Monitor your child's overall health.";
            }
        }

        /// <summary>
        /// Calculate child's age in months
        /// </summary>
        private int CalculateAgeInMonths(DateTime birthDate)
        {
            var today = DateTime.Now;
            var months = ((today.Year - birthDate.Year) * 12) + today.Month - birthDate.Month;

            if (today.Day < birthDate.Day)
                months--;

            return months < 0 ? 0 : months;
        }

        #endregion
    }
}