using Microsoft.Extensions.Logging;
using MomEase.core.DTOS.SleepRecordDTO;
using MomEase.core.Entities;
using MomEase.core.Interfaces;
using System.Globalization;

namespace MomEase.infra.Services
{
    public class SleepRecordService : ISleepRecordService
    {
        private readonly ISleepRecordRepository _sleepRecordRepository;
        private readonly IChildRepository _childRepository;
        private readonly ISleepReferenceRepository _sleepReferenceRepository;
        private readonly ILogger<SleepRecordService> _logger;

        public SleepRecordService(
            ISleepRecordRepository sleepRecordRepository,
            IChildRepository childRepository,
            ISleepReferenceRepository sleepReferenceRepository,
            ILogger<SleepRecordService> logger)
        {
            _sleepRecordRepository = sleepRecordRepository;
            _childRepository = childRepository;
            _sleepReferenceRepository = sleepReferenceRepository;
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

                // ✅ إضافة: التحقق من وجود سجل في نفس اليوم
                if (await _sleepRecordRepository.ExistsForDateAsync(dto.ChildId, dto.SleepDate))
                {
                    throw new InvalidOperationException(
                        $"A sleep record already exists for {dto.SleepDate:yyyy-MM-dd}. Please update the existing record instead.");
                }

                TimeSpan? sleepHours = null;
                if (!string.IsNullOrEmpty(dto.SleepHoursTotal))
                {
                    if (!TimeSpan.TryParse(dto.SleepHoursTotal, out var parsed))
                        throw new ArgumentException("Invalid sleep hours format. Use HH:mm:ss (e.g., 08:30:00)");
                    sleepHours = parsed;
                }

                // حساب عمر الطفل وجلب الـ Reference
                var ageInMonths = CalculateAgeInMonths(child.BirthDate);
                var reference = await _sleepReferenceRepository.GetByAgeAsync(ageInMonths);

                var record = new ChildSleepRecord
                {
                    ChildId = dto.ChildId,
                    SleepDate = dto.SleepDate,
                    SleepHoursTotal = sleepHours,
                    SleepRefId = reference?.SleepRefId,
                    Notes = dto.Notes
                };

                var createdRecord = await _sleepRecordRepository.AddSleepRecordAsync(record);
                _logger.LogInformation("Sleep record created {RecordId} for child {ChildId}", createdRecord.RecordId, dto.ChildId);

                return MapToDto(createdRecord, reference);
            }
            catch (Exception ex) when (ex is KeyNotFoundException || ex is ArgumentException ||
                                       ex is InvalidOperationException || ex is UnauthorizedAccessException)
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

                // ✅ إضافة: التحقق من التاريخ المكرر عند التحديث
                if (dto.SleepDate.HasValue && dto.SleepDate.Value.Date != record.SleepDate.Date)
                {
                    if (await _sleepRecordRepository.ExistsForDateAsync(record.ChildId, dto.SleepDate.Value, recordId))
                    {
                        throw new InvalidOperationException(
                            $"A sleep record already exists for {dto.SleepDate.Value:yyyy-MM-dd}");
                    }
                }

                if (dto.SleepDate.HasValue)
                {
                    if (dto.SleepDate.Value > DateTime.Now)
                        throw new ArgumentException("Sleep date cannot be in the future");
                    record.SleepDate = dto.SleepDate.Value;
                }

                if (!string.IsNullOrEmpty(dto.SleepHoursTotal))
                {
                    if (!TimeSpan.TryParse(dto.SleepHoursTotal, out var parsed))
                        throw new ArgumentException("Invalid sleep hours format. Use HH:mm:ss (e.g., 08:30:00)");
                    record.SleepHoursTotal = parsed;
                }

                if (dto.SleepRefId.HasValue)
                    record.SleepRefId = dto.SleepRefId;

                if (dto.Notes != null)
                    record.Notes = dto.Notes;

                var updatedRecord = await _sleepRecordRepository.UpdateSleepRecordAsync(record);
                _logger.LogInformation("Sleep record updated {RecordId}", recordId);

                // جلب الـ Reference
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
                var child = await _childRepository.GetChildByIdAsync(childId);
                if (child == null)
                    throw new KeyNotFoundException("Child not found");

                if (child.UserId != userId)
                    throw new UnauthorizedAccessException("You are not authorized to access this child's statistics");

                var records = await _sleepRecordRepository.GetChildSleepRecordsAsync(childId);
                var recordsWithSleep = records.Where(r => r.SleepHoursTotal.HasValue).ToList();

                // ✅ جلب Reference للمقارنة
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
                        CurrentSleepStatus = "No Data",
                        MostCommonStatus = "No Data",
                        ComparisonWithReference = new ComparisonWithSleepReferenceDto
                        {
                            Status = "No Data",
                            RecommendedMinHours = reference?.SleepMinHours?.TotalHours ?? 0,
                            RecommendedMaxHours = reference?.SleepMaxHours?.TotalHours ?? 0,
                            ActualAverageHours = 0,
                            Message = "No sleep records available yet"
                        }
                    };
                }

                // ✅ حساب المتوسط الكلي
                var avgTicks = (long)recordsWithSleep.Average(r => r.SleepHoursTotal.Value.Ticks);
                var avgSleep = new TimeSpan(avgTicks);

                // ✅ حساب Last 7 Days Average
                var last7Days = await _sleepRecordRepository.GetLastNDaysAsync(childId, 7);
                var last7DaysWithSleep = last7Days.Where(r => r.SleepHoursTotal.HasValue).ToList();

                TimeSpan last7DaysAvgTimeSpan = TimeSpan.Zero;
                double last7DaysAverage = 0;

                if (last7DaysWithSleep.Any())
                {
                    var last7AvgTicks = (long)last7DaysWithSleep.Average(r => r.SleepHoursTotal.Value.Ticks);
                    last7DaysAvgTimeSpan = new TimeSpan(last7AvgTicks);
                    last7DaysAverage = last7DaysWithSleep.Average(r => r.SleepHoursTotal.Value.TotalHours);
                }

                // ✅ حساب Good, Normal, Poor Days
                var goodSleep = recordsWithSleep.Count(r =>
                    reference != null &&
                    reference.SleepMinHours.HasValue &&
                    reference.SleepMaxHours.HasValue &&
                    r.SleepHoursTotal.Value.TotalHours >= reference.SleepMinHours.Value.TotalHours &&
                    r.SleepHoursTotal.Value.TotalHours <= reference.SleepMaxHours.Value.TotalHours
                );

                var poorSleep = recordsWithSleep.Count(r =>
                    reference != null &&
                    reference.SleepMinHours.HasValue &&
                    r.SleepHoursTotal.Value.TotalHours < reference.SleepMinHours.Value.TotalHours
                );

                var normalSleep = recordsWithSleep.Count(r =>
                    reference != null &&
                    reference.SleepMaxHours.HasValue &&
                    r.SleepHoursTotal.Value.TotalHours > reference.SleepMaxHours.Value.TotalHours
                );

                // ✅ حساب Most Common Status
                var statusCounts = new Dictionary<string, int>
        {
            { "Good", goodSleep },
            { "Poor", poorSleep },
            { "Normal", normalSleep }
        };
                var mostCommonStatus = statusCounts.OrderByDescending(kvp => kvp.Value).FirstOrDefault().Key ?? "Unknown";

                // ✅ Current Sleep Status (based on last 7 days)
                var currentStatus = GetSleepStatusCategory(last7DaysAverage, reference);

                // ✅ Comparison with Reference
                var comparison = new ComparisonWithSleepReferenceDto
                {
                    Status = currentStatus,
                    RecommendedMinHours = reference?.SleepMinHours?.TotalHours ?? 0,
                    RecommendedMaxHours = reference?.SleepMaxHours?.TotalHours ?? 0,
                    ActualAverageHours = Math.Round(last7DaysAverage, 1),
                    Message = GenerateComparisonMessage(last7DaysAverage, reference)
                };

                return new SleepStatisticsDto
                {
                    TotalRecords = recordsWithSleep.Count,
                    AverageSleepHours = avgSleep,
                    AverageSleepHoursFormatted = FormatTimeSpan(avgSleep),
                    MaxSleepHours = recordsWithSleep.Max(r => r.SleepHoursTotal.Value),
                    MinSleepHours = recordsWithSleep.Min(r => r.SleepHoursTotal.Value),
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

                var weekEnd = today;  // النهاردة
                var weekStart = today.AddDays(-6);  // قبل 6 أيام (total = 7 days)

                var records = await _sleepRecordRepository.GetSleepRecordsByDateRangeAsync(childId, weekStart, weekEnd);

                // ✅ جلب Reference للمقارنة
                var ageInMonths = CalculateAgeInMonths(child.BirthDate);
                var reference = await _sleepReferenceRepository.GetByAgeAsync(ageInMonths);

                var dailySleep = Enumerable.Range(0, 7).Select(i =>
                {
                    var date = weekStart.AddDays(i);
                    var dayRecord = records.FirstOrDefault(r => r.SleepDate.Date == date);
                    return new DailySleepDto
                    {
                        Date = date,
                        SleepHours = dayRecord?.SleepHoursTotal,
                        Status = GetSleepStatus(dayRecord?.SleepHoursTotal, reference)
                    };
                }).ToList();

                var avgTicks = records.Where(r => r.SleepHoursTotal.HasValue)
                                      .Average(r => (double?)r.SleepHoursTotal.Value.Ticks) ?? 0;

                return new WeeklySleepDto
                {
                    WeekStart = weekStart,
                    WeekEnd = weekEnd,
                    DailySleep = dailySleep,
                    WeeklyAverageSleep = new TimeSpan((long)avgTicks),
                    TotalRecords = records.Count
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

                var today = DateTime.Now;
                var monthStart = new DateTime(today.Year, today.Month, 1);
                var monthEnd = monthStart.AddMonths(1).AddDays(-1);

                var records = await _sleepRecordRepository.GetSleepRecordsByDateRangeAsync(childId, monthStart, monthEnd);

                // ✅ جلب Reference للمقارنة
                var ageInMonths = CalculateAgeInMonths(child.BirthDate);
                var reference = await _sleepReferenceRepository.GetByAgeAsync(ageInMonths);

                var daysInMonth = DateTime.DaysInMonth(today.Year, today.Month);
                var dailySleep = Enumerable.Range(1, daysInMonth).Select(day =>
                {
                    var date = new DateTime(today.Year, today.Month, day);
                    var dayRecord = records.FirstOrDefault(r => r.SleepDate.Date == date);
                    return new DailySleepDto
                    {
                        Date = date,
                        SleepHours = dayRecord?.SleepHoursTotal,
                        Status = GetSleepStatus(dayRecord?.SleepHoursTotal, reference)
                    };
                }).ToList();

                var avgTicks = records.Where(r => r.SleepHoursTotal.HasValue)
                                      .Average(r => (double?)r.SleepHoursTotal.Value.Ticks) ?? 0;

                var goodDays = records.Count(r =>
                    r.SleepHoursTotal.HasValue &&
                    reference != null &&
                    reference.SleepMinHours.HasValue &&
                    reference.SleepMaxHours.HasValue &&
                    r.SleepHoursTotal.Value.TotalHours >= reference.SleepMinHours.Value.TotalHours &&
                    r.SleepHoursTotal.Value.TotalHours <= reference.SleepMaxHours.Value.TotalHours
                );

                var poorDays = records.Count(r =>
                    r.SleepHoursTotal.HasValue &&
                    reference != null &&
                    reference.SleepMinHours.HasValue &&
                    r.SleepHoursTotal.Value.TotalHours < reference.SleepMinHours.Value.TotalHours
                );

                return new MonthlySleepDto
                {
                    Year = today.Year,
                    Month = today.Month,
                    MonthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(today.Month),
                    DailySleep = dailySleep,
                    MonthlyAverageSleep = new TimeSpan((long)avgTicks),
                    TotalRecords = records.Count,
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
                SleepHoursTotal = record.SleepHoursTotal,
                SleepHoursTotalFormatted = FormatTimeSpan(record.SleepHoursTotal),
                SleepRefId = record.SleepRefId,
                Notes = record.Notes,
                Status = GetSleepStatus(record.SleepHoursTotal, reference),
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
        private string GetSleepStatusCategory(double actualHours, SleepReference? reference)
        {
            if (reference == null || !reference.SleepMinHours.HasValue || !reference.SleepMaxHours.HasValue)
                return "No Reference";

            var min = reference.SleepMinHours.Value.TotalHours;
            var max = reference.SleepMaxHours.Value.TotalHours;

            if (actualHours >= min && actualHours <= max)
                return "Good";
            else if (actualHours < min)
                return "Poor";
            else
                return "Normal";
        }

        /// <summary>
        /// ✅ إضافة: Generate comparison message (مثل Feeding)
        /// </summary>
        private string GenerateComparisonMessage(double actualHours, SleepReference? reference)
        {
            if (reference == null || !reference.SleepMinHours.HasValue || !reference.SleepMaxHours.HasValue)
                return "No reference data available for this age range";

            var min = reference.SleepMinHours.Value.TotalHours;
            var max = reference.SleepMaxHours.Value.TotalHours;

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