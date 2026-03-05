using Microsoft.Extensions.Logging;
using MomEase.core.DTOS.GrowthReport;
using MomEase.core.Entities;
using MomEase.core.Enums;
using MomEase.core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace MomEase.infra.Services
{
    public class GrowthReportService : IGrowthReportService
    {
        private readonly IGrowthReportRepository _reportRepo;
        private readonly IChildRepository _childRepo;
        private readonly IGrowthRecordRepository _growthRepo;
        private readonly ISleepRecordRepository _sleepRepo;
        private readonly IFeedingRecordRepository _feedingRepo;
        private readonly IGrowthPercentileReferenceRepository _growthRefRepo;
        private readonly ISleepReferenceRepository _sleepRefRepo;
        private readonly IFeedingReferenceRepository _feedingRefRepo;
        private readonly ILogger<GrowthReportService> _logger;

        public GrowthReportService(
            IGrowthReportRepository reportRepo,
            IChildRepository childRepo,
            IGrowthRecordRepository growthRepo,
            ISleepRecordRepository sleepRepo,
            IFeedingRecordRepository feedingRepo,
            IGrowthPercentileReferenceRepository growthRefRepo,
            ISleepReferenceRepository sleepRefRepo,
            IFeedingReferenceRepository feedingRefRepo,
            ILogger<GrowthReportService> logger)
        {
            _reportRepo = reportRepo;
            _childRepo = childRepo;
            _growthRepo = growthRepo;
            _sleepRepo = sleepRepo;
            _feedingRepo = feedingRepo;
            _growthRefRepo = growthRefRepo;
            _sleepRefRepo = sleepRefRepo;
            _feedingRefRepo = feedingRefRepo;
            _logger = logger;
        }

        // =====================================================
        // PUBLIC METHODS
        // =====================================================

        public async Task<GrowthReportDto> GenerateReportAsync(
            int childId, int userId, CreateGrowthReportDto dto)
        {
            try
            {
                var child = await _childRepo.GetChildByIdAsync(childId);
                if (child == null || child.UserId != userId)
                    throw new UnauthorizedAccessException("Access denied");

                DateTime periodStart, periodEnd;

                if (dto.LastMonths.HasValue)
                {
                    if (dto.PeriodStart.HasValue || dto.PeriodEnd.HasValue)
                        throw new InvalidOperationException(
                            "Cannot use both 'lastMonths' and 'periodStart/periodEnd'.");

                    periodEnd = DateTime.Now;
                    periodStart = periodEnd.AddMonths(-dto.LastMonths.Value);
                }
                else if (dto.PeriodStart.HasValue && dto.PeriodEnd.HasValue)
                {
                    periodStart = dto.PeriodStart.Value;
                    periodEnd = dto.PeriodEnd.Value;

                    if (periodStart >= periodEnd)
                        throw new InvalidOperationException(
                            "Period start must be before period end.");
                }
                else
                {
                    throw new InvalidOperationException(
                        "You must provide either 'lastMonths' OR 'periodStart' and 'periodEnd'.");
                }

                var childAgeInDays = (DateTime.Now - child.BirthDate).TotalDays;
                var requestedPeriodInDays = (periodEnd - periodStart).TotalDays;

                if (requestedPeriodInDays > childAgeInDays)
                    throw new InvalidOperationException(
                        $"Cannot generate report for period longer than child's age. " +
                        $"Child is {(int)childAgeInDays} days old.");

                if (periodStart < child.BirthDate)
                    throw new InvalidOperationException(
                        "Report period cannot start before child's birth date.");

                var growthList = (await _growthRepo.GetByDateRangeAsync(childId, periodStart, periodEnd)).ToList();
                var sleepList = (await _sleepRepo.GetSleepRecordsByDateRangeAsync(childId, periodStart, periodEnd)).ToList();
                var feedingList = (await _feedingRepo.GetByDateRangeAsync(childId, periodStart, periodEnd)).ToList();

                var growthAnalysis = await AnalyzeGrowthAsync(growthList, child);
                var sleepAnalysis = await AnalyzeSleepAsync(sleepList, child);
                var feedingAnalysis = await AnalyzeFeedingAsync(feedingList, child);
                var correlations = AnalyzeCorrelations(growthList, sleepList, feedingList);
                var recommendations = GenerateRecommendations(growthAnalysis, sleepAnalysis, feedingAnalysis, correlations);

                var reportContent = new GrowthReportContentDto
                {
                    Summary = new ReportSummaryDto
                    {
                        OverallStatus = DetermineOverallStatus(growthAnalysis, sleepAnalysis, feedingAnalysis),
                        TotalDays = (int)(periodEnd - periodStart).TotalDays,
                        GrowthRecordsCount = growthList.Count,
                        SleepRecordsCount = sleepList.Count,
                        FeedingRecordsCount = feedingList.Count,
                        KeyInsight = GenerateKeyInsight(growthAnalysis, sleepAnalysis, feedingAnalysis, correlations)
                    },
                    GrowthAnalysis = growthAnalysis,
                    SleepAnalysis = sleepAnalysis,
                    FeedingAnalysis = feedingAnalysis,
                    Correlations = correlations,
                    Charts = await GenerateChartsDataAsync(growthList, sleepList, feedingList, child),
                    Recommendations = recommendations
                };

                var report = new GrowthReports
                {
                    ChildId = childId,
                    PeriodStart = periodStart,
                    PeriodEnd = periodEnd,
                    GrowthStatus = reportContent.Summary.OverallStatus,
                    ReportContent = JsonSerializer.Serialize(reportContent)
                };

                var savedReport = await _reportRepo.CreateAsync(report);

                _logger.LogInformation("Growth report {ReportId} generated for child {ChildId}",
                    savedReport.ReportId, childId);

                return MapToDto(savedReport, child, reportContent);
            }
            catch (Exception ex) when (ex is UnauthorizedAccessException || ex is InvalidOperationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate growth report for child {ChildId}", childId);
                throw new InvalidOperationException($"Failed to generate growth report: {ex.Message}", ex);
            }
        }

        public async Task<GrowthReportDto> GetReportByIdAsync(int reportId, int childId, int userId)
        {
            try
            {
                var child = await _childRepo.GetChildByIdAsync(childId);
                if (child == null || child.UserId != userId)
                    throw new UnauthorizedAccessException("Access denied");

                var report = await _reportRepo.GetByIdAsync(reportId);
                if (report == null)
                    throw new KeyNotFoundException("Report not found");

                if (report.ChildId != childId)
                    throw new InvalidOperationException("Report does not belong to this child");

                var content = JsonSerializer.Deserialize<GrowthReportContentDto>(report.ReportContent);
                return MapToDto(report, child, content);
            }
            catch (Exception ex) when (ex is UnauthorizedAccessException ||
                                       ex is KeyNotFoundException ||
                                       ex is InvalidOperationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve report {ReportId}", reportId);
                throw new InvalidOperationException($"Failed to retrieve report: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<GrowthReportDto>> GetAllReportsAsync(int childId, int userId)
        {
            try
            {
                var child = await _childRepo.GetChildByIdAsync(childId);
                if (child == null || child.UserId != userId)
                    throw new UnauthorizedAccessException("Access denied");

                var reports = await _reportRepo.GetByChildIdAsync(childId);
                return reports.Select(r =>
                {
                    var content = JsonSerializer.Deserialize<GrowthReportContentDto>(r.ReportContent);
                    return MapToDto(r, child, content);
                });
            }
            catch (UnauthorizedAccessException) { throw; }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve reports for child {ChildId}", childId);
                throw new InvalidOperationException($"Failed to retrieve reports: {ex.Message}", ex);
            }
        }

        public async Task<GrowthReportDto> GetLatestReportAsync(int childId, int userId)
        {
            try
            {
                var child = await _childRepo.GetChildByIdAsync(childId);
                if (child == null || child.UserId != userId)
                    throw new UnauthorizedAccessException("Access denied");

                var report = await _reportRepo.GetLatestByChildIdAsync(childId);
                if (report == null) return null;

                var content = JsonSerializer.Deserialize<GrowthReportContentDto>(report.ReportContent);
                return MapToDto(report, child, content);
            }
            catch (UnauthorizedAccessException) { throw; }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve latest report for child {ChildId}", childId);
                throw new InvalidOperationException($"Failed to retrieve latest report: {ex.Message}", ex);
            }
        }

        public async Task<bool> DeleteReportAsync(int reportId, int childId, int userId)
        {
            try
            {
                var child = await _childRepo.GetChildByIdAsync(childId);
                if (child == null || child.UserId != userId)
                    throw new UnauthorizedAccessException("Access denied");

                var belongs = await _reportRepo.BelongsToChildAsync(reportId, childId);
                if (!belongs)
                    throw new KeyNotFoundException("Report not found");

                var deleted = await _reportRepo.DeleteAsync(reportId);
                if (deleted)
                    _logger.LogInformation("Report {ReportId} deleted by user {UserId}", reportId, userId);

                return deleted;
            }
            catch (Exception ex) when (ex is UnauthorizedAccessException || ex is KeyNotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete report {ReportId}", reportId);
                throw new InvalidOperationException($"Failed to delete report: {ex.Message}", ex);
            }
        }

        // =====================================================
        // ANALYSIS METHODS
        // =====================================================

        private async Task<GrowthAnalysisDto> AnalyzeGrowthAsync(
            List<GrowthRecords> records, Child child)
        {
            if (!records.Any())
                return new GrowthAnalysisDto
                {
                    TotalWeightGain = 0,
                    TotalHeightGain = 0,
                    MonthlyWeightGainAverage = 0,
                    MonthlyHeightGainAverage = 0,
                    WeightStatus = "No Data",
                    HeightStatus = "No Data",
                    Trend = "No Data",
                    MonthlyBreakdown = new List<MonthlyGrowthSummaryDto>()
                };

            var ordered = records.OrderBy(r => r.RecordDate).ToList();
            var first = ordered.First();
            var last = ordered.Last();

            var totalWeightGain = (last.WeightKg ?? 0) - (first.WeightKg ?? 0);
            var totalHeightGain = (last.HeightCm ?? 0) - (first.HeightCm ?? 0);

            var ageSpanMonths =
                ((last.RecordDate.Year - first.RecordDate.Year) * 12)
                + last.RecordDate.Month - first.RecordDate.Month;
            if (ageSpanMonths == 0) ageSpanMonths = 1;

            var weightStatus = await DetermineWeightStatusAsync(last.WeightKg ?? 0, child);
            var heightStatus = await DetermineHeightStatusAsync(last.HeightCm ?? 0, child);
            var trend = totalWeightGain > 0 ? "Increasing"
                             : totalWeightGain < 0 ? "Decreasing" : "Stable";

            var monthlyBreakdown = await CalculateMonthlyBreakdownAsync(ordered, child);

            return new GrowthAnalysisDto
            {
                TotalWeightGain = totalWeightGain,
                TotalHeightGain = totalHeightGain,
                MonthlyWeightGainAverage = Math.Round(totalWeightGain / (decimal)ageSpanMonths, 2),
                MonthlyHeightGainAverage = Math.Round(totalHeightGain / (decimal)ageSpanMonths, 2),
                WeightStatus = weightStatus,
                HeightStatus = heightStatus,
                Trend = trend,
                MonthlyBreakdown = monthlyBreakdown
            };
        }

        private async Task<SleepAnalysisDto> AnalyzeSleepAsync(
            List<ChildSleepRecord> records, Child child)
        {
            if (!records.Any())
                return new SleepAnalysisDto
                {
                    AverageSleepHours = 0,
                    GoodSleepDays = 0,
                    PoorSleepDays = 0,
                    CurrentStatus = "No Data",
                    Message = "No sleep data available for the specified period"
                };

            var withSleep = records.Where(r => r.SleepHoursTotal.HasValue).ToList();
            if (!withSleep.Any())
                return new SleepAnalysisDto
                {
                    AverageSleepHours = 0,
                    GoodSleepDays = 0,
                    PoorSleepDays = 0,
                    CurrentStatus = "No Data",
                    Message = "No sleep hours data available"
                };

            var avgSleep = withSleep.Average(r => r.SleepHoursTotal.Value.TotalHours);
            var ageInMonths = CalculateAgeInMonths(child.BirthDate);
            var reference = await _sleepRefRepo.GetByAgeMonthsAsync(ageInMonths);

            var minSleep = reference?.SleepMinHours.HasValue == true
                ? reference.SleepMinHours.Value.TotalHours : 12;
            var maxSleep = reference?.SleepMaxHours.HasValue == true
                ? reference.SleepMaxHours.Value.TotalHours : 16;

            var goodDays = withSleep.Count(r =>
                r.SleepHoursTotal.Value.TotalHours >= minSleep &&
                r.SleepHoursTotal.Value.TotalHours <= maxSleep);
            var poorDays = withSleep.Count(r =>
                r.SleepHoursTotal.Value.TotalHours < minSleep);

            var status = avgSleep >= minSleep && avgSleep <= maxSleep ? "Good"
                       : avgSleep < minSleep ? "Poor" : "Normal";

            return new SleepAnalysisDto
            {
                AverageSleepHours = Math.Round(avgSleep, 1),
                GoodSleepDays = goodDays,
                PoorSleepDays = poorDays,
                CurrentStatus = status,
                Message = GenerateSleepMessage(avgSleep, minSleep, maxSleep)
            };
        }

        private async Task<FeedingAnalysisDto> AnalyzeFeedingAsync(
            List<ChildFeedingRecord> records, Child child)
        {
            if (!records.Any())
                return new FeedingAnalysisDto
                {
                    AverageFeedingsPerDay = 0,
                    GoodFeedingDays = 0,
                    PoorFeedingDays = 0,
                    CurrentStatus = "No Data",
                    Message = "No feeding data available for the specified period"
                };

            var uniqueDays = records.Select(r => r.FeedingDate.Date).Distinct().Count();
            var avgFeedingsPerDay = (double)records.Count / uniqueDays;
            var ageInMonths = CalculateAgeInMonths(child.BirthDate);

            var latestType = records
                .OrderByDescending(r => r.FeedingDate)
                .Select(r => r.FeedingTypeForBaby)
                .FirstOrDefault();

            var reference = await _feedingRefRepo.GetByAgeAndTypeAsync(ageInMonths, latestType);
            var minFeedings = reference?.MinTimesPerDay ?? 4;
            var maxFeedings = reference?.MaxTimesPerDay ?? 6;

            var dailyGroups = records.GroupBy(r => r.FeedingDate.Date);
            var goodDays = dailyGroups.Count(g => g.Count() >= minFeedings && g.Count() <= maxFeedings);
            var poorDays = dailyGroups.Count(g => g.Count() < minFeedings);

            var status = avgFeedingsPerDay >= minFeedings && avgFeedingsPerDay <= maxFeedings ? "Good"
                       : avgFeedingsPerDay < minFeedings ? "Poor" : "Normal";

            return new FeedingAnalysisDto
            {
                AverageFeedingsPerDay = Math.Round(avgFeedingsPerDay, 1),
                GoodFeedingDays = goodDays,
                PoorFeedingDays = poorDays,
                CurrentStatus = status,
                Message = GenerateFeedingMessage(avgFeedingsPerDay, minFeedings, maxFeedings)
            };
        }

        private CorrelationAnalysisDto AnalyzeCorrelations(
            List<GrowthRecords> growthRecords,
            List<ChildSleepRecord> sleepRecords,
            List<ChildFeedingRecord> feedingRecords)
        {
            var sleepCorr = AnalyzeSleepGrowthCorrelation(growthRecords, sleepRecords);
            var feedingCorr = AnalyzeFeedingGrowthCorrelation(growthRecords, feedingRecords);

            return new CorrelationAnalysisDto
            {
                SleepAndGrowth = sleepCorr,
                FeedingAndGrowth = feedingCorr,
                OverallInsight = GenerateOverallInsight(sleepCorr, feedingCorr)
            };
        }

        private SleepGrowthCorrelationDto AnalyzeSleepGrowthCorrelation(
            List<GrowthRecords> growthRecords, List<ChildSleepRecord> sleepRecords)
        {
            if (!growthRecords.Any() || !sleepRecords.Any())
                return new SleepGrowthCorrelationDto
                {
                    HasCorrelation = false,
                    Type = "None",
                    Message = "Not enough data to analyze the relationship between sleep and growth"
                };

            var ordered = growthRecords.OrderBy(r => r.RecordDate).ToList();
            var monthlyData = new List<(double avgSleep, decimal weightGain)>();

            for (int i = 0; i < ordered.Count - 1; i++)
            {
                var curr = ordered[i];
                var next = ordered[i + 1];
                var weightGain = (next.WeightKg ?? 0) - (curr.WeightKg ?? 0);

                var sleepInPeriod = sleepRecords
                    .Where(s => s.SleepDate >= curr.RecordDate &&
                                s.SleepDate < next.RecordDate &&
                                s.SleepHoursTotal.HasValue)
                    .ToList();

                if (sleepInPeriod.Any())
                    monthlyData.Add((
                        sleepInPeriod.Average(s => s.SleepHoursTotal.Value.TotalHours),
                        weightGain));
            }

            if (monthlyData.Count < 2)
                return new SleepGrowthCorrelationDto
                {
                    HasCorrelation = false,
                    Type = "None",
                    Message = "Not enough data to analyze the relationship"
                };

            var goodSleep = monthlyData.Where(d => d.avgSleep >= 12).ToList();
            var poorSleep = monthlyData.Where(d => d.avgSleep < 12).ToList();

            if (!goodSleep.Any() || !poorSleep.Any())
                return new SleepGrowthCorrelationDto
                {
                    HasCorrelation = false,
                    Type = "None",
                    Message = "No clear relationship observed between sleep and growth"
                };

            var goodGrowth = goodSleep.Average(d => (double)d.weightGain);
            var poorGrowth = poorSleep.Average(d => (double)d.weightGain);

            if (poorGrowth != 0 && goodGrowth > poorGrowth * 1.1)
            {
                var pct = Math.Round((goodGrowth - poorGrowth) / Math.Abs(poorGrowth) * 100, 0);
                return new SleepGrowthCorrelationDto
                {
                    HasCorrelation = true,
                    Type = "Positive",
                    Message = $"During periods of good sleep (12+ hours), growth was {pct}% better"
                };
            }

            return new SleepGrowthCorrelationDto
            {
                HasCorrelation = false,
                Type = "None",
                Message = "No clear relationship observed between sleep and growth"
            };
        }

        private FeedingGrowthCorrelationDto AnalyzeFeedingGrowthCorrelation(
            List<GrowthRecords> growthRecords, List<ChildFeedingRecord> feedingRecords)
        {
            if (!growthRecords.Any() || !feedingRecords.Any())
                return new FeedingGrowthCorrelationDto
                {
                    HasCorrelation = false,
                    Type = "None",
                    Message = "Not enough data to analyze the relationship between feeding and growth"
                };

            var ordered = growthRecords.OrderBy(r => r.RecordDate).ToList();
            var monthlyData = new List<(double avgFeedings, decimal weightGain)>();

            for (int i = 0; i < ordered.Count - 1; i++)
            {
                var curr = ordered[i];
                var next = ordered[i + 1];
                var weightGain = (next.WeightKg ?? 0) - (curr.WeightKg ?? 0);

                var feedingsInPeriod = feedingRecords
                    .Where(f => f.FeedingDate >= curr.RecordDate &&
                                f.FeedingDate < next.RecordDate)
                    .ToList();

                if (feedingsInPeriod.Any())
                {
                    var days = (next.RecordDate - curr.RecordDate).TotalDays;
                    if (days > 0)
                        monthlyData.Add((feedingsInPeriod.Count / days, weightGain));
                }
            }

            if (monthlyData.Count < 2)
                return new FeedingGrowthCorrelationDto
                {
                    HasCorrelation = false,
                    Type = "None",
                    Message = "Not enough data to analyze the relationship"
                };

            var goodFeeding = monthlyData.Where(d => d.avgFeedings >= 5).ToList();
            var poorFeeding = monthlyData.Where(d => d.avgFeedings < 5).ToList();

            if (!goodFeeding.Any() || !poorFeeding.Any())
                return new FeedingGrowthCorrelationDto
                {
                    HasCorrelation = false,
                    Type = "None",
                    Message = "No clear relationship observed between feeding and growth"
                };

            var goodGrowth = goodFeeding.Average(d => (double)d.weightGain);
            var poorGrowth = poorFeeding.Average(d => (double)d.weightGain);

            if (poorGrowth != 0 && goodGrowth > poorGrowth * 1.1)
                return new FeedingGrowthCorrelationDto
                {
                    HasCorrelation = true,
                    Type = "Positive",
                    Message = "Days with regular feeding (5+ times) showed better growth"
                };

            return new FeedingGrowthCorrelationDto
            {
                HasCorrelation = false,
                Type = "None",
                Message = "No clear relationship observed between feeding and growth"
            };
        }

        // =====================================================
        // HELPER METHODS
        // =====================================================

        private async Task<string> DetermineWeightStatusAsync(decimal weight, Child child)
        {
            var ageInMonths = CalculateAgeInMonths(child.BirthDate);
            var reference = await _growthRefRepo.GetByGenderAgeAndMetricAsync(
                child.Gender, ageInMonths, MetricType.Weight);

            if (reference == null) return "Normal";
            if (weight < (reference.P5 ?? 0)) return "Underweight";
            if (weight > (reference.P95 ?? 9999)) return "Overweight";
            if (weight < (reference.P25 ?? 0)) return "Below Average";
            if (weight > (reference.P75 ?? 9999)) return "Above Average";
            return "Normal";
        }

        private async Task<string> DetermineHeightStatusAsync(decimal height, Child child)
        {
            var ageInMonths = CalculateAgeInMonths(child.BirthDate);
            var reference = await _growthRefRepo.GetByGenderAgeAndMetricAsync(
                child.Gender, ageInMonths, MetricType.Height);

            if (reference == null) return "Normal";
            if (height < (reference.P5 ?? 0)) return "Short";
            if (height > (reference.P95 ?? 9999)) return "Tall";
            if (height < (reference.P25 ?? 0)) return "Below Average";
            if (height > (reference.P75 ?? 9999)) return "Above Average";
            return "Normal";
        }

        private async Task<List<MonthlyGrowthSummaryDto>> CalculateMonthlyBreakdownAsync(
            List<GrowthRecords> ordered, Child child)
        {
            var breakdown = new List<MonthlyGrowthSummaryDto>();

            for (int i = 0; i < ordered.Count - 1; i++)
            {
                var curr = ordered[i];
                var next = ordered[i + 1];
                var weightGain = (next.WeightKg ?? 0) - (curr.WeightKg ?? 0);
                var heightGain = (next.HeightCm ?? 0) - (curr.HeightCm ?? 0);

                var daysBetween = (next.RecordDate - curr.RecordDate).TotalDays;
                var monthsBetween = daysBetween / 30.0;

                var ageAtRecord = CalculateAgeInMonthsAtDate(child.BirthDate, curr.RecordDate);
                var refCurr = await _growthRefRepo.GetByGenderAgeAndMetricAsync(
                    child.Gender, ageAtRecord, MetricType.Weight);
                var refNext = await _growthRefRepo.GetByGenderAgeAndMetricAsync(
                    child.Gender, ageAtRecord + 1, MetricType.Weight);

                // الزيادة الطبيعية المتوقعة بين الشهرين من الـ DB
                var expectedMonthlyGain = (refCurr != null && refNext != null)
                    ? (refNext.P50 ?? 0) - (refCurr.P50 ?? 0)
                    : 0.5m; // fallback

                var expectedGain = expectedMonthlyGain * (decimal)monthsBetween;
                var status = weightGain >= expectedGain * 0.75m ? "Good" : "Poor";

                breakdown.Add(new MonthlyGrowthSummaryDto
                {
                    Month = curr.RecordDate.ToString("MMMM yyyy"),
                    WeightGain = Math.Round(weightGain, 2),
                    HeightGain = Math.Round(heightGain, 2),
                    Status = status
                });
            }

            return breakdown;
        }

        private async Task<ReportChartsDto> GenerateChartsDataAsync(
            List<GrowthRecords> growthRecords,
            List<ChildSleepRecord> sleepRecords,
            List<ChildFeedingRecord> feedingRecords,
            Child child)
        {
            var weightChart = growthRecords
                .Where(r => r.WeightKg.HasValue)
                .OrderBy(r => r.RecordDate)
                .Select(r => new ChartDataPointDto
                {
                    Date = r.RecordDate,
                    AgeMonths = CalculateAgeInMonthsAtDate(child.BirthDate, r.RecordDate),
                    Value = r.WeightKg.Value,
                    Label = $"{r.WeightKg.Value:F1} kg"
                }).ToList();

            var heightChart = growthRecords
                .Where(r => r.HeightCm.HasValue)
                .OrderBy(r => r.RecordDate)
                .Select(r => new ChartDataPointDto
                {
                    Date = r.RecordDate,
                    AgeMonths = CalculateAgeInMonthsAtDate(child.BirthDate, r.RecordDate),
                    Value = r.HeightCm.Value,
                    Label = $"{r.HeightCm.Value:F1} cm"
                }).ToList();

            var sleepChart = sleepRecords
                .Where(r => r.SleepHoursTotal.HasValue)
                .GroupBy(r => r.SleepDate.Date)
                .Select(g => new ChartDataPointDto
                {
                    Date = g.Key,
                    AgeMonths = CalculateAgeInMonthsAtDate(child.BirthDate, g.Key),
                    Value = (decimal)g.First().SleepHoursTotal.Value.TotalHours,
                    Label = $"{g.First().SleepHoursTotal.Value.TotalHours:F1}h"
                })
                .OrderBy(c => c.Date).ToList();

            var feedingChart = feedingRecords
                .GroupBy(r => r.FeedingDate.Date)
                .Select(g => new ChartDataPointDto
                {
                    Date = g.Key,
                    AgeMonths = CalculateAgeInMonthsAtDate(child.BirthDate, g.Key),
                    Value = g.Count(),
                    Label = $"{g.Count()} feedings"
                })
                .OrderBy(c => c.Date).ToList();

            var weightPercentiles = new List<PercentileLineDto>();
            foreach (var r in growthRecords.Where(r => r.WeightKg.HasValue).OrderBy(r => r.RecordDate))
            {
                var age = CalculateAgeInMonthsAtDate(child.BirthDate, r.RecordDate);
                var ref_ = await _growthRefRepo.GetByGenderAgeAndMetricAsync(child.Gender, age, MetricType.Weight);
                weightPercentiles.Add(new PercentileLineDto
                {
                    Date = r.RecordDate,
                    AgeMonths = age,
                    P5 = ref_?.P5 ?? 0,
                    P95 = ref_?.P95 ?? 0
                });
            }

            var heightPercentiles = new List<PercentileLineDto>();
            foreach (var r in growthRecords.Where(r => r.HeightCm.HasValue).OrderBy(r => r.RecordDate))
            {
                var age = CalculateAgeInMonthsAtDate(child.BirthDate, r.RecordDate);
                var ref_ = await _growthRefRepo.GetByGenderAgeAndMetricAsync(child.Gender, age, MetricType.Height);
                heightPercentiles.Add(new PercentileLineDto
                {
                    Date = r.RecordDate,
                    AgeMonths = age,
                    P5 = ref_?.P5 ?? 0,
                    P95 = ref_?.P95 ?? 0
                });
            }

            var sleepReference = new List<ReferenceRangePointDto>();
            foreach (var r in sleepRecords.Where(r => r.SleepHoursTotal.HasValue)
                                          .GroupBy(r => r.SleepDate.Date)
                                          .Select(g => g.First())
                                          .OrderBy(r => r.SleepDate))
            {
                var age = CalculateAgeInMonthsAtDate(child.BirthDate, r.SleepDate);
                var ref_ = await _sleepRefRepo.GetByAgeMonthsAsync(age);
                sleepReference.Add(new ReferenceRangePointDto
                {
                    Date = r.SleepDate.Date,
                    AgeMonths = age,
                    Min = ref_?.SleepMinHours.HasValue == true ? (decimal)ref_.SleepMinHours.Value.TotalHours : 0,
                    Max = ref_?.SleepMaxHours.HasValue == true ? (decimal)ref_.SleepMaxHours.Value.TotalHours : 0
                });
            }

            var latestType = feedingRecords
                .OrderByDescending(r => r.FeedingDate)
                .Select(r => r.FeedingTypeForBaby)
                .FirstOrDefault();

            var feedingReference = new List<ReferenceRangePointDto>();
            foreach (var g in feedingRecords.GroupBy(r => r.FeedingDate.Date).OrderBy(g => g.Key))
            {
                var age = CalculateAgeInMonthsAtDate(child.BirthDate, g.Key);
                var ref_ = await _feedingRefRepo.GetByAgeAndTypeAsync(age, latestType);
                feedingReference.Add(new ReferenceRangePointDto
                {
                    Date = g.Key,
                    AgeMonths = age,
                    Min = ref_?.MinTimesPerDay ?? 0,
                    Max = ref_?.MaxTimesPerDay ?? 0
                });
            }

            return new ReportChartsDto
            {
                WeightChart = weightChart,
                HeightChart = heightChart,
                SleepChart = sleepChart,
                FeedingChart = feedingChart,
                WeightPercentiles = weightPercentiles,
                HeightPercentiles = heightPercentiles,
                SleepReference = sleepReference,
                FeedingReference = feedingReference
            };
        }

        private string DetermineOverallStatus(
            GrowthAnalysisDto growth, SleepAnalysisDto sleep, FeedingAnalysisDto feeding)
        {
            var scores = new List<string> { growth.WeightStatus, sleep.CurrentStatus, feeding.CurrentStatus };
            var goodCount = scores.Count(s => s is "Good" or "Normal" or "Above Average" or "Below Average");
            var poorCount = scores.Count(s => s is "Poor" or "Underweight" or "Overweight" or "Short" or "Tall");

            if (goodCount >= 2) return "Excellent - Normal Growth";
            if (poorCount >= 2) return "Needs Attention";
            return "Good";
        }

        private string GenerateKeyInsight(
            GrowthAnalysisDto growth, SleepAnalysisDto sleep,
            FeedingAnalysisDto feeding, CorrelationAnalysisDto correlations)
        {
            if (growth.WeightStatus is "Underweight" or "Overweight")
                return $"⚠️ Attention: Your child's weight is {growth.WeightStatus}, follow-up with a doctor is recommended";
            if (sleep.CurrentStatus == "Poor")
                return "⚠️ Your child needs more sleep for better growth";
            if (feeding.CurrentStatus == "Poor")
                return "⚠️ Number of feedings is below recommended";
            if (correlations.SleepAndGrowth.HasCorrelation)
                return "✅ Good sleep helps with better growth - keep it up!";
            return "✅ Your child is growing normally";
        }

        private string GenerateOverallInsight(
            SleepGrowthCorrelationDto sleepCorr, FeedingGrowthCorrelationDto feedingCorr)
        {
            if (sleepCorr.HasCorrelation && feedingCorr.HasCorrelation)
                return "Good sleep and regular feeding together contribute to better growth";
            if (sleepCorr.HasCorrelation)
                return "Good sleep has a positive impact on your child's growth";
            if (feedingCorr.HasCorrelation)
                return "Regular feeding has a positive impact on your child's growth";
            return "Continue with current sleep and feeding routine";
        }

        private List<string> GenerateRecommendations(
            GrowthAnalysisDto growth, SleepAnalysisDto sleep,
            FeedingAnalysisDto feeding, CorrelationAnalysisDto correlations)
        {
            var recs = new List<string>();

            if (sleep.CurrentStatus == "Poor")
            {
                recs.Add("🌙 Try to increase your child's sleep hours to the normal range");
                recs.Add("🌙 Maintain a regular sleep routine");
            }
            else if (sleep.CurrentStatus == "Good")
                recs.Add("✅ Continue with the current sleep routine - it's excellent");

            if (feeding.CurrentStatus == "Poor")
            {
                recs.Add("🍼 Try to increase the number of feedings");
                recs.Add("🍼 Make sure the baby feeds adequately each time");
            }
            else if (feeding.CurrentStatus == "Good")
                recs.Add("✅ Current feeding routine is excellent - keep it up");

            if (growth.WeightStatus == "Underweight")
            {
                recs.Add("⚠️ Your child's weight is below normal - consult a pediatrician");
                recs.Add("⚠️ Ensure adequate feeding");
            }
            else if (growth.WeightStatus == "Overweight")
                recs.Add("⚠️ Your child's weight is above normal - consult a doctor");
            else
                recs.Add("✅ Your child's growth is normal - continue with the same routine");

            if (correlations.SleepAndGrowth.HasCorrelation)
                recs.Add("💡 Good sleep helps your child's growth - make it a priority");

            recs.Add("📊 Monitor progress next month and compare with this report");
            return recs;
        }

        private string GenerateSleepMessage(double avg, double min, double max)
        {
            if (avg < min * 0.7)
                return $"⚠️⚠️ Average sleep ({avg:F1}h) is significantly below recommended ({min}-{max}h). Please consult a pediatrician";
            if (avg < min)
                return $"⚠️ Average sleep ({avg:F1}h) is slightly below recommended ({min}-{max}h)";
            if (avg <= max)
                return $"✅ Average sleep ({avg:F1}h) is within the normal range ({min}-{max}h)";
            return $"Average sleep ({avg:F1}h) is slightly above recommended ({min}-{max}h), which is usually fine";
        }

        private string GenerateFeedingMessage(double avg, int min, int max)
        {
            if (avg < min * 0.7)
                return $"⚠️⚠️ Average feedings ({avg:F1}/day) is significantly below recommended ({min}-{max}). Please consult a pediatrician";
            if (avg < min)
                return $"⚠️ Average feedings ({avg:F1}/day) is slightly below recommended ({min}-{max})";
            if (avg <= max)
                return $"✅ Average feedings ({avg:F1}/day) is within the normal range ({min}-{max})";
            return $"Average feedings ({avg:F1}/day) is above recommended ({min}-{max}), which is usually good";
        }

        private GrowthReportDto MapToDto(GrowthReports report, Child child, GrowthReportContentDto content)
        {
            return new GrowthReportDto
            {
                ReportId = report.ReportId,
                ChildId = report.ChildId,
                ChildName = child.FullName,
                PeriodStart = report.PeriodStart,
                PeriodEnd = report.PeriodEnd,
                GrowthStatus = report.GrowthStatus,
                CreatedAt = DateTime.Now,
                ReportContent = content
            };
        }

        private int CalculateAgeInMonths(DateTime birthDate)
        {
            var today = DateTime.Now;
            var months = ((today.Year - birthDate.Year) * 12) + today.Month - birthDate.Month;
            if (today.Day < birthDate.Day) months--;
            return months < 0 ? 0 : months;
        }

        private int CalculateAgeInMonthsAtDate(DateTime birthDate, DateTime atDate)
        {
            var months = ((atDate.Year - birthDate.Year) * 12) + atDate.Month - birthDate.Month;
            if (atDate.Day < birthDate.Day) months--;
            return months < 0 ? 0 : months;
        }
    }
}