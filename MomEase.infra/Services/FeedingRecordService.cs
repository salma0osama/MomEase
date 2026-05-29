using Microsoft.AspNetCore.Http;
using MomEase.core.DTOS.FeedingRecordDto;
using MomEase.core.Entities;
using MomEase.core.Enums;
using MomEase.core.Interfaces;
using MomEase.infra.Helpers;
namespace MomEase.infra.Services
{
    public class FeedingRecordService : IFeedingRecordService
    {
        private readonly IFeedingRecordRepository _feedingRecordRepository;
        private readonly IFeedingReferenceRepository _feedingReferenceRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IChildRepository _childRepository;

        public FeedingRecordService(
            IFeedingRecordRepository feedingRecordRepository,
            IFeedingReferenceRepository feedingReferenceRepository,
            IHttpContextAccessor httpContextAccessor,
            IChildRepository childRepository)
        {
            _feedingRecordRepository = feedingRecordRepository ?? throw new ArgumentNullException(nameof(feedingRecordRepository));
            _feedingReferenceRepository = feedingReferenceRepository ?? throw new ArgumentNullException(nameof(feedingReferenceRepository));
            _childRepository = childRepository ?? throw new ArgumentNullException(nameof(childRepository));
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<FeedingRecordResponseDto> CreateFeedingRecordAsync(int childId, CreateFeedingRecordDto createDto)
        {
            try
            {
                var child = await _childRepository.GetChildByIdAsync(childId);
                if (child == null)
                    throw new KeyNotFoundException($"Child with ID {childId} not found");

                if (createDto.FeedingDate.Date > DateTime.Now.Date)
                    throw new ArgumentException("Cannot create feeding record for future dates");

                var ageInMonths = CalculateAgeInMonths(child.BirthDate);

                if (ageInMonths > 24)
                    throw new InvalidOperationException("Feeding tracking is only available for children up to 24 months old");

                // Check if same type exists on same date
                if (await _feedingRecordRepository.ExistsForDateAndTypeAsync(childId, createDto.FeedingDate, createDto.FeedingTypeForBaby))
                    throw new InvalidOperationException($"A {createDto.FeedingTypeForBaby} feeding record already exists for {createDto.FeedingDate:yyyy-MM-dd}. Please update the existing record instead.");

                var reference = await _feedingReferenceRepository.GetByAgeAndTypeAsync(ageInMonths, createDto.FeedingTypeForBaby);

                var feedingType = CalculateFeedingType(
                    createDto.FeedingTimesPerDay,
                    reference?.MinTimesPerDay,
                    reference?.MaxTimesPerDay
                );

                var feedingRecord = new ChildFeedingRecord
                {
                    ChildId = childId,
                    FeedingDate = createDto.FeedingDate.Date,
                    FeedingTimesPerDay = createDto.FeedingTimesPerDay,
                    FeedingTypeForBaby = createDto.FeedingTypeForBaby,
                    FeedingType = feedingType,
                    FeedingRefId = reference?.FeedingRefId,
                    Notes = createDto.Notes
                };

                await _feedingRecordRepository.AddAsync(feedingRecord);
                await _feedingRecordRepository.SaveChangesAsync();

                return MapToResponseDto(feedingRecord, child, reference);
            }
            catch (Exception ex) when (ex is KeyNotFoundException || ex is ArgumentException || ex is InvalidOperationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to create feeding record: {ex.Message}", ex);
            }
        }

        public async Task<List<FeedingRecordResponseDto>> GetAllRecordsForChildAsync(int childId)
        {
            try
            {
                var child = await _childRepository.GetChildByIdAsync(childId);
                if (child == null)
                    throw new KeyNotFoundException($"Child with ID {childId} not found");

                var records = await _feedingRecordRepository.GetByChildIdAsync(childId);

                return records.Select(r => MapToResponseDto(r, child, r.FeedingReference)).ToList();
            }
            catch (Exception ex) when (ex is KeyNotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to retrieve feeding records: {ex.Message}", ex);
            }
        }

        public async Task<FeedingRecordResponseDto> GetRecordByIdAsync(int recordId)
        {
            try
            {
                var record = await _feedingRecordRepository.GetByIdAsync(recordId);

                if (record == null)
                    throw new KeyNotFoundException($"Feeding record with ID {recordId} not found");

                return MapToResponseDto(record, record.Child, record.FeedingReference);
            }
            catch (Exception ex) when (ex is KeyNotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to retrieve feeding record: {ex.Message}", ex);
            }
        }

        public async Task<FeedingRecordResponseDto> UpdateFeedingRecordAsync(int recordId, UpdateFeedingRecordDto updateDto)
        {
            try
            {
                var record = await _feedingRecordRepository.GetByIdAsync(recordId);

                if (record == null)
                    throw new KeyNotFoundException($"Feeding record with ID {recordId} not found");

                if (record.FeedingDate.Date != updateDto.FeedingDate.Date || record.FeedingTypeForBaby != updateDto.FeedingTypeForBaby)
                {
                    if (await _feedingRecordRepository.ExistsForDateAndTypeAsync(record.ChildId, updateDto.FeedingDate, updateDto.FeedingTypeForBaby, recordId))
                        throw new InvalidOperationException($"A {updateDto.FeedingTypeForBaby} record already exists for {updateDto.FeedingDate:yyyy-MM-dd}");
                }

                var child = record.Child;
                var ageInMonths = CalculateAgeInMonths(child.BirthDate);

                var reference = await _feedingReferenceRepository.GetByAgeAndTypeAsync(ageInMonths, updateDto.FeedingTypeForBaby);

                var feedingType = CalculateFeedingType(
                    updateDto.FeedingTimesPerDay,
                    reference?.MinTimesPerDay,
                    reference?.MaxTimesPerDay
                );

                record.FeedingDate = updateDto.FeedingDate.Date;
                record.FeedingTimesPerDay = updateDto.FeedingTimesPerDay;
                record.FeedingTypeForBaby = updateDto.FeedingTypeForBaby;
                record.FeedingType = feedingType;
                record.FeedingRefId = reference?.FeedingRefId;
                record.Notes = updateDto.Notes;

                await _feedingRecordRepository.UpdateAsync(record);
                await _feedingRecordRepository.SaveChangesAsync();

                return MapToResponseDto(record, child, reference);
            }
            catch (Exception ex) when (ex is KeyNotFoundException || ex is InvalidOperationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to update feeding record: {ex.Message}", ex);
            }
        }

        public async Task<bool> DeleteFeedingRecordAsync(int recordId)
        {
            try
            {
                var record = await _feedingRecordRepository.GetByIdAsync(recordId);

                if (record == null)
                    throw new KeyNotFoundException($"Feeding record with ID {recordId} not found");

                await _feedingRecordRepository.DeleteAsync(record);
                await _feedingRecordRepository.SaveChangesAsync();

                return true;
            }
            catch (Exception ex) when (ex is KeyNotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to delete feeding record: {ex.Message}", ex);
            }
        }

        public async Task<FeedingStatisticsDto> GetFeedingStatisticsAsync(int childId)
        {
            try
            {
                var lang = LanguageHelper.GetLang(_httpContextAccessor);
                var child = await _childRepository.GetChildByIdAsync(childId);
                if (child == null)
                    throw new KeyNotFoundException($"Child with ID {childId} not found");

                var allRecords = await _feedingRecordRepository.GetByChildIdAsync(childId);

                if (!allRecords.Any())
                {
                    return new FeedingStatisticsDto
                    {
                        TotalRecords = 0,
                        AverageTimesPerDay = 0,
                        Last7DaysAverage = 0,
                        CurrentFeedingStatus = lang == "ar" ? "لا توجد بيانات" : "No Data",
                        MostCommonFeedingType = lang == "ar" ? "غير محدد" : "N/A",
                        ComparisonWithReference = new ComparisonWithReferenceDto
                        {
                            Status = lang == "ar" ? "لا توجد بيانات" : "No Data",
                            RecommendedMin = 0,
                            RecommendedMax = 0,
                            ActualAverage = 0,
                            Message = lang == "ar"
                                ? "لا توجد سجلات تغذية متاحة حتى الآن"
                                : "No feeding records available yet"
                        }
                    };
                }

                var last7Days = await _feedingRecordRepository.GetLastNDaysAsync(childId, 7);
                var totalAverage = allRecords.Average(r => r.FeedingTimesPerDay);
                var last7DaysAverage = last7Days.Any() ? last7Days.Average(r => r.FeedingTimesPerDay) : 0;

                var mostCommonType = allRecords
                    .GroupBy(r => r.FeedingType)
                    .OrderByDescending(g => g.Count())
                    .FirstOrDefault()?.Key.ToString() ?? (lang == "ar" ? "غير محدد" : "N/A");

                var ageInMonths = CalculateAgeInMonths(child.BirthDate);

                // Use child's primary feeding type for reference
                var reference = await _feedingReferenceRepository.GetByAgeAndTypeAsync(ageInMonths, child.FeedingTypeForBaby);

                var comparison = new ComparisonWithReferenceDto
                {
                    Status = mostCommonType,
                    RecommendedMin = reference?.MinTimesPerDay ?? 0,
                    RecommendedMax = reference?.MaxTimesPerDay ?? 0,
                    ActualAverage = last7DaysAverage,
                    Message = GenerateComparisonMessage(last7DaysAverage, reference?.MinTimesPerDay, reference?.MaxTimesPerDay, lang)
                };

                return new FeedingStatisticsDto
                {
                    TotalRecords = allRecords.Count,
                    AverageTimesPerDay = Math.Round(totalAverage, 1),
                    Last7DaysAverage = Math.Round(last7DaysAverage, 1),
                    CurrentFeedingStatus = mostCommonType,
                    MostCommonFeedingType = mostCommonType,
                    ComparisonWithReference = comparison
                };
            }
            catch (Exception ex) when (ex is KeyNotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to retrieve feeding statistics: {ex.Message}", ex);
            }
        }

        public async Task<WeeklyFeedingDto> GetWeeklyRecordsAsync(int childId)
        {
            try
            {
                var lang = LanguageHelper.GetLang(_httpContextAccessor);
                var child = await _childRepository.GetChildByIdAsync(childId);
                if (child == null)
                    throw new KeyNotFoundException($"Child with ID {childId} not found");

                var today = DateTime.Now.Date;

                // ✅ حساب أول يوم في الأسبوع (الأحد)
                var dayOfWeek = (int)today.DayOfWeek;
                var weekStart = today.AddDays(-dayOfWeek);  // الأحد
                var weekEnd = weekStart.AddDays(6);  // السبت

                var records = await _feedingRecordRepository.GetByDateRangeAsync(childId, weekStart, weekEnd);

                var dailyRecords = new List<DailyFeedingDto>();

                for (int i = 0; i < 7; i++)
                {
                    var date = weekStart.AddDays(i);
                    var dayRecords = records.Where(r => r.FeedingDate.Date == date).ToList();

                    if (dayRecords.Any())
                    {
                        // ✅ إذا كان فيه أكثر من سجل في نفس اليوم، خد أول واحد
                        var record = dayRecords.First();
                        dailyRecords.Add(new DailyFeedingDto
                        {
                            Date = date,
                            TimesPerDay = record.FeedingTimesPerDay,
                            FeedingType = lang == "ar" ? GetArabicFeedingType(record.FeedingTypeForBaby) : record.FeedingTypeForBaby.ToString(),
                            Status = GetFeedingStatus(record.FeedingType, lang)
                        });
                    }
                    else
                    {
                        dailyRecords.Add(new DailyFeedingDto
                        {
                            Date = date,
                            TimesPerDay = null,
                            FeedingType = null,
                            Status = null
                        });
                    }
                }

                var weeklyAverage = records.Any() ? records.Average(r => r.FeedingTimesPerDay) : 0;

                return new WeeklyFeedingDto
                {
                    WeekStart = weekStart,
                    WeekEnd = weekEnd,
                    DailyRecords = dailyRecords,
                    WeeklyAverage = Math.Round(weeklyAverage, 1)
                };
            }
            catch (Exception ex) when (ex is KeyNotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to retrieve weekly feeding records: {ex.Message}", ex);
            }
        }

        public async Task<MonthlyFeedingDto> GetMonthlyRecordsAsync(int childId)
        {
            try
            {
                var lang = LanguageHelper.GetLang(_httpContextAccessor);
                var child = await _childRepository.GetChildByIdAsync(childId);
                if (child == null)
                    throw new KeyNotFoundException($"Child with ID {childId} not found");

                var records = await _feedingRecordRepository.GetCurrentMonthAsync(childId);

                if (!records.Any())
                {
                    var now = DateTime.Now;
                    return new MonthlyFeedingDto
                    {
                        Month = now.Month,
                        Year = now.Year,
                        MonthName = lang == "ar"
                            ? GetArabicMonthName(now.Month)
                            : now.ToString("MMMM yyyy"),
                        TotalRecords = 0,
                        AverageTimesPerDay = 0,
                        FeedingTypeDistribution = new Dictionary<string, int>(),
                        DominantStatus = lang == "ar" ? "لا توجد بيانات" : "No Data"
                    };
                }

                var feedingTypeDistribution = records
                    .GroupBy(r => r.FeedingType.ToString())
                    .ToDictionary(g => g.Key, g => g.Count());

                var dominantStatus = feedingTypeDistribution
                    .OrderByDescending(kvp => kvp.Value)
                    .FirstOrDefault().Key ?? (lang == "ar" ? "غير معروف" : "Unknown");

                var currentMonth = DateTime.Now;

                return new MonthlyFeedingDto
                {
                    Month = currentMonth.Month,
                    Year = currentMonth.Year,
                    MonthName = lang == "ar"
                        ? GetArabicMonthName(currentMonth.Month)
                        : currentMonth.ToString("MMMM yyyy"),
                    TotalRecords = records.Count,
                    AverageTimesPerDay = Math.Round(records.Average(r => r.FeedingTimesPerDay), 1),
                    FeedingTypeDistribution = feedingTypeDistribution,
                    DominantStatus = GetFeedingStatus(Enum.Parse<FeedingType>(dominantStatus), lang)
                };
            }
            catch (Exception ex) when (ex is KeyNotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to retrieve monthly feeding records: {ex.Message}", ex);
            }
        }

        #region Helper Methods

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

        /// <summary>
        /// Calculate feeding type based on actual vs recommended times
        /// </summary>
        private FeedingType CalculateFeedingType(int actualTimes, int? minTimes, int? maxTimes)
        {
            if (!minTimes.HasValue || !maxTimes.HasValue)
                return FeedingType.Normal;

            var min = minTimes.Value;
            var max = maxTimes.Value;

            // Severe Under: less than 50% of minimum
            if (actualTimes < min * 0.5)
                return FeedingType.SevereUnder;

            // Under: less than minimum
            if (actualTimes < min)
                return FeedingType.Under;

            // Normal: within recommended range
            if (actualTimes >= min && actualTimes <= max)
                return FeedingType.Normal;

            // Over: up to 150% of maximum
            if (actualTimes <= max * 1.5)
                return FeedingType.Over;

            // Obese: more than 150% of maximum
            return FeedingType.Obese;
        }

        /// <summary>
        /// Generate user-friendly comparison message
        /// </summary>
        private string GenerateComparisonMessage(double actual, int? min, int? max, string lang = "en")
        {
            if (!min.HasValue || !max.HasValue)
                return lang == "ar"
                    ? "لا توجد بيانات مرجعية متاحة لهذه الفئة العمرية"
                    : "No reference data available for this age range";

            if (lang == "ar")
            {
                // العربية
                if (actual < min * 0.5)
                    return $"⚠️ تكرار التغذية أقل بكثير من الموصى به ({min}-{max} مرات/يوم). يرجى استشارة طبيب الأطفال.";

                if (actual < min)
                    return $"⚠️ تكرار التغذية أقل قليلاً من الموصى به ({min}-{max} مرات/يوم).";

                if (actual >= min && actual <= max)
                    return $"✅ الطفل يتغذى ضمن النطاق الموصى به ({min}-{max} مرات/يوم).";

                if (actual <= max * 1.5)
                    return $"⚠️ تكرار التغذية أعلى قليلاً من الموصى به ({min}-{max} مرات/يوم).";

                return $"⚠️ تكرار التغذية أعلى بكثير من الموصى به ({min}-{max} مرات/يوم). يرجى استشارة طبيب الأطفال.";
            }
            else
            {
                // الإنجليزية
                if (actual < min * 0.5)
                    return $"⚠️ Feeding frequency is significantly below recommended ({min}-{max} times/day). Please consult with a pediatrician.";

                if (actual < min)
                    return $"⚠️ Feeding frequency is slightly below recommended ({min}-{max} times/day).";

                if (actual >= min && actual <= max)
                    return $"✅ Your baby is feeding within the recommended range ({min}-{max} times/day).";

                if (actual <= max * 1.5)
                    return $"⚠️ Feeding frequency is slightly above recommended ({min}-{max} times/day).";

                return $"⚠️ Feeding frequency is significantly above recommended ({min}-{max} times/day). Please consult with a pediatrician.";
            }
        }

        /// <summary>
        /// Map entity to response DTO
        /// </summary>
        private FeedingRecordResponseDto MapToResponseDto(ChildFeedingRecord record, Child child, FeedingReference? reference)
        {
            return new FeedingRecordResponseDto
            {
                RecordId = record.RecordId,
                ChildId = record.ChildId,
                ChildName = child.FullName,
                FeedingDate = record.FeedingDate,
                FeedingTimesPerDay = record.FeedingTimesPerDay,
                FeedingTypeForBaby = record.FeedingTypeForBaby.ToString(),
                FeedingType = record.FeedingType.ToString(),
                Notes = record.Notes,
                ReferenceInfo = reference != null ? new FeedingReferenceInfo
                {
                    MinTimesPerDay = reference.MinTimesPerDay ?? 0,
                    MaxTimesPerDay = reference.MaxTimesPerDay ?? 0,
                    AgeRange = $"{reference.AgeMinMonths}-{reference.AgeMaxMonths} months"
                } : null
            };
        }
        private string GetFeedingStatus(FeedingType type, string lang = "en")
        {
            if (lang == "ar")
            {
                return type switch
                {
                    FeedingType.SevereUnder => "أقل بكثير من المطلوب",
                    FeedingType.Under => "أقل من المطلوب",
                    FeedingType.Normal => "عادي",
                    FeedingType.Over => "أكثر من المطلوب",
                    FeedingType.Obese => "أكثر بكثير من المطلوب",
                    _ => "غير معروف"
                };
            }
            else
            {
                return type switch
                {
                    FeedingType.SevereUnder => "Severely Under",
                    FeedingType.Under => "Under",
                    FeedingType.Normal => "Normal",
                    FeedingType.Over => "Over",
                    FeedingType.Obese => "Severely Over",
                    _ => "Unknown"
                };
            }
        }
        private string GetArabicFeedingType(FeedingTypeForBaby type)
        {
            return type switch
            {
                FeedingTypeForBaby.Breastfeeding => "رضاعة طبيعية",
                FeedingTypeForBaby.Formula => "رضاعة صناعية",
                FeedingTypeForBaby.SolidFood => "رضاعة مختلطة",
                _ => type.ToString()
            };
        }
        private string GetArabicMonthName(int month)
        {
            var arabicMonths = new[]
            {
                "يناير",    // January
                "فبراير",   // February
                "مارس",     // March
                "أبريل",    // April
                "مايو",     // May
                "يونيو",    // June
                "يوليو",    // July
                "أغسطس",    // August
                "سبتمبر",   // September
                "أكتوبر",   // October
                "نوفمبر",   // November
                "ديسمبر"    // December
            };

            return month >= 1 && month <= 12 ? arabicMonths[month - 1] : "Unknown";
        }
        #endregion
    }
}
