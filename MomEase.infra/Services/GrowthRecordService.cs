using MomEase.core.DTOS.GrowthTracking;
using MomEase.core.Entities;
using MomEase.core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MomEase.infra.Services
{
    public class GrowthRecordService : IGrowthRecordService
    {
        private readonly IGrowthRecordRepository _growthRepo;
        private readonly IChildRepository _childRepo;

        public GrowthRecordService(IGrowthRecordRepository growthRepo, IChildRepository childRepo)
        {
            _growthRepo = growthRepo;
            _childRepo = childRepo;
        }

        public async Task<GrowthRecordDto> CreateAsync(int childId, int userId, CreateGrowthRecordDto dto)
        {
            var child = await _childRepo.GetChildByIdAsync(childId);
            if (child == null || child.UserId != userId)
                throw new UnauthorizedAccessException("Access denied");

            // ✅ التحقق من عدم وجود record في نفس اليوم
            var recordDate = DateTime.Now.AddHours(1);
            var alreadyExists = await _growthRepo.ExistsForDateAsync(childId, recordDate);
            if (alreadyExists)
                throw new InvalidOperationException("A growth record already exists for today. You can update the existing record instead.");

            var ageInWeeks = (int)((recordDate - child.BirthDate).TotalDays / 7);

            var record = new GrowthRecords
            {
                ChildId = childId,
                RecordDate = recordDate,
                AgeInWeeks = ageInWeeks,
                WeightKg = dto.WeightKg,
                HeightCm = dto.HeightCm
            };

            var created = await _growthRepo.CreateAsync(record);

            return new GrowthRecordDto
            {
                GrowthId = created.GrowthId,
                ChildName = child.FullName,
                RecordDate = created.RecordDate,
                AgeInWeeks = created.AgeInWeeks,
                WeightKg = created.WeightKg.Value,
                HeightCm = created.HeightCm.Value
            };
        }

        public async Task<GrowthRecordDto> GetByIdAsync(int growthId, int childId, int userId)
        {
            var child = await _childRepo.GetChildByIdAsync(childId);
            if (child == null || child.UserId != userId)
                throw new UnauthorizedAccessException("Access denied");

            var record = await _growthRepo.GetByIdAsync(growthId);
            if (record == null || record.ChildId != childId)
                throw new Exception("Record not found");

            return new GrowthRecordDto
            {
                GrowthId = record.GrowthId,
                ChildName = child.FullName,
                RecordDate = record.RecordDate,
                AgeInWeeks = record.AgeInWeeks,
                WeightKg = record.WeightKg.Value,
                HeightCm = record.HeightCm.Value
            };
        }

        public async Task<IEnumerable<GrowthRecordDto>> GetAllAsync(int childId, int userId)
        {
            var child = await _childRepo.GetChildByIdAsync(childId);
            if (child == null || child.UserId != userId)
                throw new UnauthorizedAccessException("Access denied");

            var records = await _growthRepo.GetByChildIdAsync(childId);

            return records.Select(r => new GrowthRecordDto
            {
                GrowthId = r.GrowthId,
                ChildName = child.FullName,
                RecordDate = r.RecordDate,
                AgeInWeeks = r.AgeInWeeks,
                WeightKg = r.WeightKg.Value,
                HeightCm = r.HeightCm.Value
            });
        }

        public async Task<GrowthRecordDto> UpdateAsync(int growthId, int childId, int userId, UpdateGrowthRecordDto dto)
        {
            var child = await _childRepo.GetChildByIdAsync(childId);
            if (child == null || child.UserId != userId)
                throw new UnauthorizedAccessException("Access denied");

            var record = await _growthRepo.GetByIdAsync(growthId);
            if (record == null || record.ChildId != childId)
                throw new Exception("Record not found");

            if (dto.WeightKg.HasValue)
                record.WeightKg = dto.WeightKg.Value;

            if (dto.HeightCm.HasValue)
                record.HeightCm = dto.HeightCm.Value;

            var updated = await _growthRepo.UpdateAsync(record);

            return new GrowthRecordDto
            {
                GrowthId = updated.GrowthId,
                ChildName = child.FullName,
                RecordDate = updated.RecordDate,
                AgeInWeeks = updated.AgeInWeeks,
                WeightKg = updated.WeightKg.Value,
                HeightCm = updated.HeightCm.Value
            };
        }

        public async Task<bool> DeleteAsync(int growthId, int childId, int userId)
        {
            var child = await _childRepo.GetChildByIdAsync(childId);
            if (child == null || child.UserId != userId)
                throw new UnauthorizedAccessException("Access denied");

            var belongs = await _growthRepo.BelongsToChildAsync(growthId, childId);
            if (!belongs)
                throw new Exception("Record not found");

            return await _growthRepo.DeleteAsync(growthId);
        }

        public async Task<GrowthChartDto> GetChartDataAsync(int childId, int userId)
        {
            var child = await _childRepo.GetChildByIdAsync(childId);
            if (child == null || child.UserId != userId)
                throw new UnauthorizedAccessException("Access denied");

            var records = await _growthRepo.GetByChildIdAsync(childId);

            return new GrowthChartDto
            {
                ChildName = child.FullName,
                WeightData = records
                    .Where(r => r.WeightKg.HasValue)
                    .OrderBy(r => r.RecordDate)
                    .Select(r => new ChartPoint
                    {
                        Date = r.RecordDate,
                        AgeInWeeks = r.AgeInWeeks,
                        Value = r.WeightKg.Value
                    }).ToList(),
                HeightData = records
                    .Where(r => r.HeightCm.HasValue)
                    .OrderBy(r => r.RecordDate)
                    .Select(r => new ChartPoint
                    {
                        Date = r.RecordDate,
                        AgeInWeeks = r.AgeInWeeks,
                        Value = r.HeightCm.Value
                    }).ToList()
            };
        }
        public async Task<GrowthStatisticsDto> GetStatisticsAsync(int childId, int userId)
        {
            var child = await _childRepo.GetChildByIdAsync(childId);
            if (child == null || child.UserId != userId)
                throw new UnauthorizedAccessException("Access denied");

            var records = await _growthRepo.GetByChildIdAsync(childId);
            var recordsList = records.ToList();

            if (!recordsList.Any())
            {
                return new GrowthStatisticsDto
                {
                    TotalRecords = 0,
                    CurrentGrowthStatus = "No Data",
                    WeightTrend = "No Data",
                    HeightTrend = "No Data"
                };
            }

            // حساب الإحصائيات
            var weights = recordsList.Where(r => r.WeightKg.HasValue).Select(r => r.WeightKg.Value).ToList();
            var heights = recordsList.Where(r => r.HeightCm.HasValue).Select(r => r.HeightCm.Value).ToList();

            var firstRecord = recordsList.OrderBy(r => r.RecordDate).First();
            var lastRecord = recordsList.OrderByDescending(r => r.RecordDate).First();

            var weightGain = lastRecord.WeightKg - firstRecord.WeightKg;
            var heightGain = lastRecord.HeightCm - firstRecord.HeightCm;

            var ageSpanMonths = (lastRecord.RecordDate - firstRecord.RecordDate).TotalDays / 30;
            if (ageSpanMonths < 1) ageSpanMonths = 1;

            return new GrowthStatisticsDto
            {
                TotalRecords = recordsList.Count,
                AverageWeight = weights.Any() ? weights.Average() : 0,
                MaxWeight = weights.Any() ? weights.Max() : 0,
                MinWeight = weights.Any() ? weights.Min() : 0,
                WeightGainTotal = weightGain ?? 0,
                MonthlyWeightGainAverage = ageSpanMonths > 0 ? (weightGain ?? 0) / (decimal)ageSpanMonths : 0,

                AverageHeight = heights.Any() ? heights.Average() : 0,
                MaxHeight = heights.Any() ? heights.Max() : 0,
                MinHeight = heights.Any() ? heights.Min() : 0,
                HeightGainTotal = heightGain ?? 0,
                MonthlyHeightGainAverage = ageSpanMonths > 0 ? (heightGain ?? 0) / (decimal)ageSpanMonths : 0,

                CurrentGrowthStatus = "Normal", // TODO: Compare with percentiles
                WeightTrend = weightGain > 0 ? "Increasing" : weightGain < 0 ? "Decreasing" : "Stable",
                HeightTrend = heightGain > 0 ? "Increasing" : heightGain < 0 ? "Decreasing" : "Stable"
            };
        }

        public async Task<WeeklyGrowthDto> GetWeeklyGrowthAsync(int childId, int userId)
        {
            var child = await _childRepo.GetChildByIdAsync(childId);
            if (child == null || child.UserId != userId)
                throw new UnauthorizedAccessException("Access denied");

            var today = DateTime.Now.AddHours(1).Date;
            var dayOfWeek = (int)today.DayOfWeek;
            var weekStart = today.AddDays(-dayOfWeek);
            var weekEnd = weekStart.AddDays(6);

            var records = await _growthRepo.GetByDateRangeAsync(childId, weekStart, weekEnd);
            var recordsList = records.ToList();

            var dailyGrowth = Enumerable.Range(0, 7).Select(i =>
            {
                var date = weekStart.AddDays(i);
                var dayRecord = recordsList.FirstOrDefault(r => r.RecordDate.Date == date);
                return new DailyGrowthDto
                {
                    Date = date,
                    Weight = dayRecord?.WeightKg,
                    Height = dayRecord?.HeightCm,
                    Status = "Normal" // TODO: Calculate based on percentiles
                };
            }).ToList();

            var firstRecord = recordsList.OrderBy(r => r.RecordDate).FirstOrDefault();
            var lastRecord = recordsList.OrderByDescending(r => r.RecordDate).FirstOrDefault();

            return new WeeklyGrowthDto
            {
                WeekStart = weekStart,
                WeekEnd = weekEnd,
                DailyGrowth = dailyGrowth,
                WeeklyWeightGain = (lastRecord?.WeightKg ?? 0) - (firstRecord?.WeightKg ?? 0),
                WeeklyHeightGain = (lastRecord?.HeightCm ?? 0) - (firstRecord?.HeightCm ?? 0),
                TotalRecords = recordsList.Count
            };
        }

        public async Task<MonthlyGrowthDto> GetMonthlyGrowthAsync(int childId, int userId)
        {
            var child = await _childRepo.GetChildByIdAsync(childId);
            if (child == null || child.UserId != userId)
                throw new UnauthorizedAccessException("Access denied");

            var today = DateTime.Now.AddHours(1);
            var monthStart = new DateTime(today.Year, today.Month, 1);
            var monthEnd = monthStart.AddMonths(1).AddDays(-1);

            var records = await _growthRepo.GetByDateRangeAsync(childId, monthStart, monthEnd);
            var recordsList = records.ToList();

            var daysInMonth = DateTime.DaysInMonth(today.Year, today.Month);
            var dailyGrowth = Enumerable.Range(1, daysInMonth).Select(day =>
            {
                var date = new DateTime(today.Year, today.Month, day);
                var dayRecord = recordsList.FirstOrDefault(r => r.RecordDate.Date == date);
                return new DailyGrowthDto
                {
                    Date = date,
                    Weight = dayRecord?.WeightKg,
                    Height = dayRecord?.HeightCm,
                    Status = "Normal"
                };
            }).ToList();

            var firstRecord = recordsList.OrderBy(r => r.RecordDate).FirstOrDefault();
            var lastRecord = recordsList.OrderByDescending(r => r.RecordDate).FirstOrDefault();

            return new MonthlyGrowthDto
            {
                Year = today.Year,
                Month = today.Month,
                MonthName = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(today.Month),
                DailyGrowth = dailyGrowth,
                MonthlyWeightGain = (lastRecord?.WeightKg ?? 0) - (firstRecord?.WeightKg ?? 0),
                MonthlyHeightGain = (lastRecord?.HeightCm ?? 0) - (firstRecord?.HeightCm ?? 0),
                TotalRecords = recordsList.Count,
                GoodGrowthDays = 0, // TODO: Calculate
                PoorGrowthDays = 0
            };
        }
    }
}
