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
            IChildRepository childRepository,
            IHttpContextAccessor httpContextAccessor)
        {
            _feedingRecordRepository = feedingRecordRepository ?? throw new ArgumentNullException(nameof(feedingRecordRepository));
            _feedingReferenceRepository = feedingReferenceRepository ?? throw new ArgumentNullException(nameof(feedingReferenceRepository));
            _childRepository = childRepository ?? throw new ArgumentNullException(nameof(childRepository));
            _httpContextAccessor = httpContextAccessor;
        }

        private static readonly Dictionary<string, string> FeedingTypeAr = new()
        {
            { "Breastfeeding", "الرضاعة الطبيعية" },
            { "Formula", "الحليب الصناعي" },
            { "SolidFood", "الطعام الصلب" }
        };

        private static readonly Dictionary<string, string> FeedingStatusAr = new()
        {
            { "Normal", "طبيعي" },
            { "Under", "أقل من المعدل" },
            { "Over", "أكثر من المعدل" },
            { "SevereUnder", "أقل بكثير من المعدل" },
            { "Obese", "مفرط" },
            { "No Data", "لا توجد بيانات" },
            { "N/A", "غير متاح" }
        };

        private static readonly Dictionary<string, string> ArabicToEnglishFeedingType = new()
        {
            { "الرضاعة الطبيعية", "Breastfeeding" },
            { "الحليب الصناعي", "Formula" },
            { "الطعام الصلب", "SolidFood" }
        };

        private string NormalizeFeedingType(string value)
        {
            if (string.IsNullOrEmpty(value)) return value;
            if (ArabicToEnglishFeedingType.ContainsKey(value))
                return ArabicToEnglishFeedingType[value];
            return value;
        }

        public async Task<FeedingRecordResponseDto> CreateFeedingRecordAsync(int childId, CreateFeedingRecordDto createDto)
        {
            try
            {
                var isAr = GetLang().StartsWith("ar");
                var normalizedType = NormalizeFeedingType(createDto.FeedingTypeForBaby);

                var validTypes = new[] { "Breastfeeding", "Formula", "SolidFood" };
                if (!validTypes.Contains(normalizedType, StringComparer.OrdinalIgnoreCase))
                    throw new ArgumentException(isAr
                        ? "نوع الرضاعة غير صحيح. القيم المقبولة: الرضاعة الطبيعية، الحليب الصناعي، الطعام الصلب"
                        : "Invalid feeding type. Valid values are: Breastfeeding, Formula, SolidFood");

                Enum.TryParse<FeedingTypeForBaby>(normalizedType, true, out var feedingTypeEnum);

                var child = await _childRepository.GetChildByIdAsync(childId);
                if (child == null)
                    throw new KeyNotFoundException($"Child with ID {childId} not found");

                if (createDto.FeedingDate.Date > DateTime.Now.Date)
                    throw new ArgumentException("Cannot create feeding record for future dates");

                var ageInMonths = CalculateAgeInMonths(child.BirthDate);

                if (ageInMonths > 24)
                    throw new InvalidOperationException("Feeding tracking is only available for children up to 24 months old");

                if (await _feedingRecordRepository.ExistsForDateAndTypeAsync(childId, createDto.FeedingDate, feedingTypeEnum))
                    throw new InvalidOperationException($"A {createDto.FeedingTypeForBaby} feeding record already exists for {createDto.FeedingDate:yyyy-MM-dd}. Please update the existing record instead.");

                var reference = await _feedingReferenceRepository.GetByAgeAndTypeAsync(ageInMonths, feedingTypeEnum);

                var feedingType = CalculateFeedingType(
                    createDto.FeedingTimesPerDay,
                    reference?.MinTimesPerDay,
                    reference?.MaxTimesPerDay);

                var feedingRecord = new ChildFeedingRecord
                {
                    ChildId = childId,
                    FeedingDate = createDto.FeedingDate.Date,
                    FeedingTimesPerDay = createDto.FeedingTimesPerDay,
                    FeedingTypeForBaby = feedingTypeEnum,
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
                var isAr = GetLang().StartsWith("ar");
                var normalizedType = NormalizeFeedingType(updateDto.FeedingTypeForBaby);

                var validTypes = new[] { "Breastfeeding", "Formula", "SolidFood" };
                if (!validTypes.Contains(normalizedType, StringComparer.OrdinalIgnoreCase))
                    throw new ArgumentException(isAr
                        ? "نوع الرضاعة غير صحيح. القيم المقبولة: الرضاعة الطبيعية، الحليب الصناعي، الطعام الصلب"
                        : "Invalid feeding type. Valid values are: Breastfeeding, Formula, SolidFood");

                Enum.TryParse<FeedingTypeForBaby>(normalizedType, true, out var feedingTypeEnum);

                var record = await _feedingRecordRepository.GetByIdAsync(recordId);
                if (record == null)
                    throw new KeyNotFoundException($"Feeding record with ID {recordId} not found");

                if (record.FeedingDate.Date != updateDto.FeedingDate.Date || record.FeedingTypeForBaby != feedingTypeEnum)
                {
                    if (await _feedingRecordRepository.ExistsForDateAndTypeAsync(
                        record.ChildId, updateDto.FeedingDate, feedingTypeEnum, recordId))
                        throw new InvalidOperationException(
                            $"A {feedingTypeEnum} record already exists for {updateDto.FeedingDate:yyyy-MM-dd}");
                }

                var child = record.Child;
                var ageInMonths = CalculateAgeInMonths(child.BirthDate);
                var reference = await _feedingReferenceRepository.GetByAgeAndTypeAsync(ageInMonths, feedingTypeEnum);

                var feedingType = CalculateFeedingType(
                    updateDto.FeedingTimesPerDay,
                    reference?.MinTimesPerDay,
                    reference?.MaxTimesPerDay);

                record.FeedingDate = updateDto.FeedingDate.Date;
                record.FeedingTimesPerDay = updateDto.FeedingTimesPerDay;
                record.FeedingTypeForBaby = feedingTypeEnum;
                record.FeedingType = feedingType;
                record.FeedingRefId = reference?.FeedingRefId;
                record.Notes = updateDto.Notes;

                await _feedingRecordRepository.UpdateAsync(record);
                await _feedingRecordRepository.SaveChangesAsync();

                return MapToResponseDto(record, child, reference);
            }
            catch (Exception ex) when (ex is KeyNotFoundException || ex is InvalidOperationException || ex is ArgumentException)
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
                var lang = GetLang();
                var isAr = lang.StartsWith("ar");

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
                        CurrentFeedingStatus = isAr ? "لا توجد بيانات" : "No Data",
                        MostCommonFeedingType = isAr ? "غير محدد" : "N/A",
                        ComparisonWithReference = new ComparisonWithReferenceDto
                        {
                            Status = isAr ? "لا توجد بيانات" : "No Data",
                            RecommendedMin = 0,
                            RecommendedMax = 0,
                            ActualAverage = 0,
                            Message = isAr
                                ? "لا توجد سجلات تغذية متاحة حتى الآن"
                                : "No feeding records available yet"
                        }
                    };
                }

                var last7Days = await _feedingRecordRepository.GetLastNDaysAsync(childId, 7);
                var totalAverage = allRecords.Average(r => r.FeedingTimesPerDay);
                var last7DaysAverage = last7Days.Any() ? last7Days.Average(r => r.FeedingTimesPerDay) : 0;

                var mostCommonFeedingType = allRecords
                    .GroupBy(r => r.FeedingTypeForBaby)
                    .OrderByDescending(g => g.Count())
                    .FirstOrDefault()?.Key.ToString() ?? "N/A";

                var currentFeedingStatus = allRecords
                    .GroupBy(r => r.FeedingType)
                    .OrderByDescending(g => g.Count())
                    .FirstOrDefault()?.Key.ToString() ?? (isAr ? "غير محدد" : "N/A");

                var ageInMonths = CalculateAgeInMonths(child.BirthDate);

                FeedingReference? reference = null;
                if (Enum.TryParse<FeedingTypeForBaby>(mostCommonFeedingType, out var dominantType))
                    reference = await _feedingReferenceRepository.GetByAgeAndTypeAsync(ageInMonths, dominantType);

                return new FeedingStatisticsDto
                {
                    TotalRecords = allRecords.Count,
                    AverageTimesPerDay = Math.Round(totalAverage, 1),
                    Last7DaysAverage = Math.Round(last7DaysAverage, 1),
                    CurrentFeedingStatus = LocalizeStatus(currentFeedingStatus),
                    MostCommonFeedingType = LocalizeFeedingType(mostCommonFeedingType),
                    ComparisonWithReference = new ComparisonWithReferenceDto
                    {
                        Status = LocalizeStatus(currentFeedingStatus),
                        RecommendedMin = reference?.MinTimesPerDay ?? 0,
                        RecommendedMax = reference?.MaxTimesPerDay ?? 0,
                        ActualAverage = last7DaysAverage,
                        Message = isAr
                            ? GenerateComparisonMessageAr(last7DaysAverage, reference?.MinTimesPerDay, reference?.MaxTimesPerDay)
                            : GenerateComparisonMessage(last7DaysAverage, reference?.MinTimesPerDay, reference?.MaxTimesPerDay)
                    }
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
                var child = await _childRepository.GetChildByIdAsync(childId);
                if (child == null)
                    throw new KeyNotFoundException($"Child with ID {childId} not found");

                var today = DateTime.Now.Date;
                var weekStart = today.AddDays(-6);
                var weekEnd = today;

                var records = await _feedingRecordRepository.GetByDateRangeAsync(childId, weekStart, weekEnd);
                var dailyRecords = new List<DailyFeedingDto>();

                for (int i = 0; i < 7; i++)
                {
                    var date = weekStart.AddDays(i);
                    var dayRecords = records.Where(r => r.FeedingDate.Date == date).ToList();

                    dailyRecords.Add(new DailyFeedingDto
                    {
                        Date = date,
                        Records = dayRecords.Select(r => new DailyFeedingEntryDto
                        {
                            TimesPerDay = r.FeedingTimesPerDay,
                            FeedingType = LocalizeFeedingType(r.FeedingTypeForBaby.ToString()),
                            Status = LocalizeStatus(r.FeedingType.ToString())
                        }).ToList()
                    });
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
                throw new InvalidOperationException($"Failed to retrieve weekly records: {ex.Message}", ex);
            }
        }

        public async Task<MonthlyFeedingDto> GetMonthlyRecordsAsync(int childId)
        {
            try
            {
                var child = await _childRepository.GetChildByIdAsync(childId);
                if (child == null)
                    throw new KeyNotFoundException($"Child with ID {childId} not found");

                var now = DateTime.Now;
                var startOfMonth = new DateTime(now.Year, now.Month, 1);
                var daysInMonth = DateTime.DaysInMonth(now.Year, now.Month);

                var records = await _feedingRecordRepository.GetCurrentMonthAsync(childId);
                var dailyRecords = new List<DailyFeedingDto>();

                for (int i = 0; i < daysInMonth; i++)
                {
                    var date = startOfMonth.AddDays(i);
                    var dayRecords = records.Where(r => r.FeedingDate.Date == date.Date).ToList();

                    dailyRecords.Add(new DailyFeedingDto
                    {
                        Date = date,
                        Records = dayRecords.Select(r => new DailyFeedingEntryDto
                        {
                            TimesPerDay = r.FeedingTimesPerDay,
                            FeedingType = LocalizeFeedingType(r.FeedingTypeForBaby.ToString()),
                            Status = LocalizeStatus(r.FeedingType.ToString())
                        }).ToList()
                    });
                }

                var monthlyAverage = records.Any() ? records.Average(r => r.FeedingTimesPerDay) : 0;
                var normalDays = records.Count(r => r.FeedingType == FeedingType.Normal);
                var abnormalDays = records.Count(r => r.FeedingType != FeedingType.Normal);

                return new MonthlyFeedingDto
                {
                    Year = now.Year,
                    Month = now.Month,
                    MonthName = now.ToString("MMMM"),
                    DailyRecords = dailyRecords,
                    MonthlyAverageTimesPerDay = Math.Round(monthlyAverage, 1),
                    TotalRecords = records.Count,
                    NormalDays = normalDays,
                    AbnormalDays = abnormalDays
                };
            }
            catch (Exception ex) when (ex is KeyNotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to retrieve monthly records: {ex.Message}", ex);
            }
        }

        #region Helper Methods

        private int CalculateAgeInMonths(DateTime birthDate)
        {
            var today = DateTime.Now;
            var months = ((today.Year - birthDate.Year) * 12) + today.Month - birthDate.Month;
            if (today.Day < birthDate.Day)
                months--;
            return months < 0 ? 0 : months;
        }

        private FeedingType CalculateFeedingType(int actualTimes, int? minTimes, int? maxTimes)
        {
            if (!minTimes.HasValue || !maxTimes.HasValue)
                return FeedingType.Normal;

            var min = minTimes.Value;
            var max = maxTimes.Value;

            if (actualTimes < min * 0.5) return FeedingType.SevereUnder;
            if (actualTimes < min) return FeedingType.Under;
            if (actualTimes <= max) return FeedingType.Normal;
            if (actualTimes <= max * 1.5) return FeedingType.Over;
            return FeedingType.Obese;
        }

        private string GenerateComparisonMessage(double actual, int? min, int? max)
        {
            if (!min.HasValue || !max.HasValue)
                return "No reference data available for this age range";

            if (actual < min * 0.5)
                return $"⚠️ Feeding frequency is significantly below recommended ({min}-{max} times/day). Please consult with a pediatrician.";
            if (actual < min)
                return $"⚠️ Feeding frequency is slightly below recommended ({min}-{max} times/day).";
            if (actual <= max)
                return $"✅ Your baby is feeding within the recommended range ({min}-{max} times/day).";
            if (actual <= max * 1.5)
                return $"⚠️ Feeding frequency is slightly above recommended ({min}-{max} times/day).";

            return $"⚠️ Feeding frequency is significantly above recommended ({min}-{max} times/day). Please consult with a pediatrician.";
        }

        private string GenerateComparisonMessageAr(double actual, int? min, int? max)
        {
            if (!min.HasValue || !max.HasValue)
                return "لا توجد بيانات مرجعية لهذه الفئة العمرية";

            if (actual < min * 0.5)
                return $"⚠️ عدد مرات الرضاعة أقل بكثير من الموصى به ({min}-{max} مرة/يوم). يرجى استشارة طبيب الأطفال.";
            if (actual < min)
                return $"⚠️ عدد مرات الرضاعة أقل قليلاً من الموصى به ({min}-{max} مرة/يوم).";
            if (actual <= max)
                return $"✅ طفلك يرضع ضمن النطاق الموصى به ({min}-{max} مرة/يوم).";
            if (actual <= max * 1.5)
                return $"⚠️ عدد مرات الرضاعة أكثر قليلاً من الموصى به ({min}-{max} مرة/يوم).";

            return $"⚠️ عدد مرات الرضاعة أكثر بكثير من الموصى به ({min}-{max} مرة/يوم). يرجى استشارة طبيب الأطفال.";
        }

        private FeedingRecordResponseDto MapToResponseDto(
            ChildFeedingRecord record, Child child, FeedingReference? reference)
        {
            return new FeedingRecordResponseDto
            {
                RecordId = record.RecordId,
                ChildId = record.ChildId,
                ChildName = child.FullName,
                FeedingDate = record.FeedingDate,
                FeedingTimesPerDay = record.FeedingTimesPerDay,
                FeedingTypeForBaby = LocalizeFeedingType(record.FeedingTypeForBaby.ToString()),
                FeedingType = LocalizeStatus(record.FeedingType.ToString()),
                Notes = record.Notes,
                ReferenceInfo = reference != null ? new FeedingReferenceInfo
                {
                    MinTimesPerDay = reference.MinTimesPerDay ?? 0,
                    MaxTimesPerDay = reference.MaxTimesPerDay ?? 0,
                    AgeRange = $"{reference.AgeMinMonths}-{reference.AgeMaxMonths} months"
                } : null
            };
        }

        private string GetLang() =>
            _httpContextAccessor.HttpContext?
                .Request.Headers["Accept-Language"]
                .ToString().ToLower() ?? "en";

        private string LocalizeFeedingType(string value)
        {
            if (GetLang().StartsWith("ar") && FeedingTypeAr.ContainsKey(value))
                return FeedingTypeAr[value];
            return value;
        }

        private string LocalizeStatus(string value)
        {
            if (GetLang().StartsWith("ar") && FeedingStatusAr.ContainsKey(value))
                return FeedingStatusAr[value];
            return value;
        }

        #endregion
    }
}