using PostCare.core.DTOS.GrowthTracking;
using PostCare.core.Entities;
using PostCare.core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PostCare.infra.Services
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

            var recordDate = DateTime.Now;
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
    }
}
