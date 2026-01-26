using Microsoft.Extensions.Logging;
using PostCare.core.DTOS.SleepRecordDTO;
using PostCare.core.Entities;
using PostCare.core.Interfaces;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PostCare.infra.Services
{
    public class SleepRecordService : ISleepRecordService
    {
        private readonly ISleepRecordRepository _sleepRecordRepository;
        private readonly IChildRepository _childRepository;
        private readonly ILogger<SleepRecordService> _logger;

        public SleepRecordService(
            ISleepRecordRepository sleepRecordRepository,
            IChildRepository childRepository,
            ILogger<SleepRecordService> logger)
        {
            _sleepRecordRepository = sleepRecordRepository;
            _childRepository = childRepository;
            _logger = logger;
        }

        public async Task<SleepRecordDto> CreateSleepRecordAsync(int userId, CreateSleepRecordDto dto)
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

            var record = new ChildSleepRecord
            {
                ChildId = dto.ChildId,
                SleepDate = dto.SleepDate,
                SleepHoursTotal = dto.SleepHoursTotal,
                SleepRefId = dto.SleepRefId,
                Notes = dto.Notes
            };

            var createdRecord = await _sleepRecordRepository.AddSleepRecordAsync(record);
            _logger.LogInformation("Sleep record created {RecordId} for child {ChildId}", createdRecord.RecordId, dto.ChildId);

            return MapToDto(createdRecord);
        }

        public async Task<List<SleepRecordDto>> GetChildSleepRecordsAsync(int childId, int userId)
        {
            var child = await _childRepository.GetChildByIdAsync(childId);
            if (child == null)
                throw new KeyNotFoundException("Child not found");

            if (child.UserId != userId)
                throw new UnauthorizedAccessException("You are not authorized to access this child's records");

            var records = await _sleepRecordRepository.GetChildSleepRecordsAsync(childId);
            return records.Select(r => MapToDto(r)).ToList();
        }

        public async Task<SleepRecordDto> GetSleepRecordByIdAsync(int recordId, int userId)
        {
            var record = await _sleepRecordRepository.GetSleepRecordByIdAsync(recordId);
            if (record == null)
                throw new KeyNotFoundException("Sleep record not found");

            if (record.Child.UserId != userId)
                throw new UnauthorizedAccessException("You are not authorized to access this record");

            return MapToDto(record);
        }

        public async Task<SleepRecordDto> UpdateSleepRecordAsync(int recordId, int userId, UpdateSleepRecordDto dto)
        {
            var record = await _sleepRecordRepository.GetSleepRecordByIdAsync(recordId);
            if (record == null)
                throw new KeyNotFoundException("Sleep record not found");

            if (record.Child.UserId != userId)
                throw new UnauthorizedAccessException("You are not authorized to update this record");

            if (dto.SleepDate.HasValue)
            {
                if (dto.SleepDate.Value > DateTime.Now)
                    throw new ArgumentException("Sleep date cannot be in the future");
                record.SleepDate = dto.SleepDate.Value;
            }

            if (dto.SleepHoursTotal.HasValue)
                record.SleepHoursTotal = dto.SleepHoursTotal;

            if (dto.SleepRefId.HasValue)
                record.SleepRefId = dto.SleepRefId;

            if (dto.Notes != null)
                record.Notes = dto.Notes;

            var updatedRecord = await _sleepRecordRepository.UpdateSleepRecordAsync(record);
            _logger.LogInformation("Sleep record updated {RecordId}", recordId);

            return MapToDto(updatedRecord);
        }

        public async Task<bool> DeleteSleepRecordAsync(int recordId, int userId)
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

        public async Task<SleepStatisticsDto> GetSleepStatisticsAsync(int childId, int userId)
        {
            var child = await _childRepository.GetChildByIdAsync(childId);
            if (child == null)
                throw new KeyNotFoundException("Child not found");

            if (child.UserId != userId)
                throw new UnauthorizedAccessException("You are not authorized to access this child's statistics");

            var records = await _sleepRecordRepository.GetChildSleepRecordsAsync(childId);
            var recordsWithSleep = records.Where(r => r.SleepHoursTotal.HasValue).ToList();

            if (!recordsWithSleep.Any())
            {
                return new SleepStatisticsDto
                {
                    TotalRecords = 0,
                    AverageSleepHours = TimeSpan.Zero,
                    AverageSleepHoursFormatted = "0h 0m"
                };
            }

            var avgTicks = (long)recordsWithSleep.Average(r => r.SleepHoursTotal.Value.Ticks);
            var avgSleep = new TimeSpan(avgTicks);

            var goodSleep = recordsWithSleep.Count(r => r.SleepHoursTotal.Value.TotalHours >= 10);
            var poorSleep = recordsWithSleep.Count(r => r.SleepHoursTotal.Value.TotalHours < 8);

            return new SleepStatisticsDto
            {
                TotalRecords = recordsWithSleep.Count,
                AverageSleepHours = avgSleep,
                AverageSleepHoursFormatted = FormatTimeSpan(avgSleep),
                MaxSleepHours = recordsWithSleep.Max(r => r.SleepHoursTotal.Value),
                MinSleepHours = recordsWithSleep.Min(r => r.SleepHoursTotal.Value),
                GoodSleepDays = goodSleep,
                PoorSleepDays = poorSleep,
                SleepQualityPercentage = (double)goodSleep / recordsWithSleep.Count * 100
            };
        }

        public async Task<WeeklySleepDto> GetWeeklySleepAsync(int childId, int userId)
        {
            var child = await _childRepository.GetChildByIdAsync(childId);
            if (child == null)
                throw new KeyNotFoundException("Child not found");

            if (child.UserId != userId)
                throw new UnauthorizedAccessException("You are not authorized to access this child's records");

            var today = DateTime.Now.Date;
            var weekStart = today.AddDays(-(int)today.DayOfWeek);
            var weekEnd = weekStart.AddDays(6);

            var records = await _sleepRecordRepository.GetSleepRecordsByDateRangeAsync(childId, weekStart, weekEnd);

            var dailySleep = Enumerable.Range(0, 7).Select(i =>
            {
                var date = weekStart.AddDays(i);
                var dayRecord = records.FirstOrDefault(r => r.SleepDate.Date == date);
                return new DailySleepDto
                {
                    Date = date,
                    SleepHours = dayRecord?.SleepHoursTotal,
                    Status = GetSleepStatus(dayRecord?.SleepHoursTotal)
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

        public async Task<MonthlySleepDto> GetMonthlySleepAsync(int childId, int userId)
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

            var daysInMonth = DateTime.DaysInMonth(today.Year, today.Month);
            var dailySleep = Enumerable.Range(1, daysInMonth).Select(day =>
            {
                var date = new DateTime(today.Year, today.Month, day);
                var dayRecord = records.FirstOrDefault(r => r.SleepDate.Date == date);
                return new DailySleepDto
                {
                    Date = date,
                    SleepHours = dayRecord?.SleepHoursTotal,
                    Status = GetSleepStatus(dayRecord?.SleepHoursTotal)
                };
            }).ToList();

            var avgTicks = records.Where(r => r.SleepHoursTotal.HasValue)
                                  .Average(r => (double?)r.SleepHoursTotal.Value.Ticks) ?? 0;

            var goodDays = records.Count(r => r.SleepHoursTotal.HasValue && r.SleepHoursTotal.Value.TotalHours >= 10);
            var poorDays = records.Count(r => r.SleepHoursTotal.HasValue && r.SleepHoursTotal.Value.TotalHours < 8);

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

        private SleepRecordDto MapToDto(ChildSleepRecord record)
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
                Status = GetSleepStatus(record.SleepHoursTotal)
            };
        }

        private string FormatTimeSpan(TimeSpan? timeSpan)
        {
            if (!timeSpan.HasValue) return "N/A";
            return $"{(int)timeSpan.Value.TotalHours}h {timeSpan.Value.Minutes}m";
        }

        private string GetSleepStatus(TimeSpan? sleepHours)
        {
            if (!sleepHours.HasValue) return "Unknown";
            var hours = sleepHours.Value.TotalHours;
            if (hours >= 10) return "Good";
            if (hours >= 8) return "Normal";
            return "Poor";
        }
    }
}
