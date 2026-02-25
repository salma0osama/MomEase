using MomEase.core.DTOS.AssessmentDto;
using MomEase.core.Entities;
using MomEase.core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MomEase.infra.Services
{
    public class AssessmentService : IAssessmentService
    {
        private readonly IAssessmentRepository _repo;

        public AssessmentService(IAssessmentRepository repo)
        {
            _repo = repo;
        }

        public async Task<IEnumerable<AssessmentDto>> GetAllAsync()
        {
            var list = await _repo.GetAllAsync();
            return list.Select(MapToDto);
        }

        public async Task<AssessmentDto?> GetByIdAsync(int id)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null) return null;
            return MapToDto(entity);
        }

        public async Task<AssessmentDto> CreateAsync(CreateAssessmentDto dto)
        {
            var entity = new Assessment
            {
                Name = dto.Name,
                Description = dto.Description,
                TotalQuestions = dto.TotalQuestions,
                MaxScore = dto.MaxScore
            };

            var created = await _repo.CreateAsync(entity);
            return MapToDto(created);
        }

        public async Task<AssessmentDto?> UpdateAsync(int id, UpdateAssessmentDto dto)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null) return null;

            // Partial update — بنغير بس اللي اتبعت
            if (dto.Name != null) entity.Name = dto.Name;
            if (dto.Description != null) entity.Description = dto.Description;
            if (dto.TotalQuestions != null) entity.TotalQuestions = dto.TotalQuestions.Value;
            if (dto.MaxScore != null) entity.MaxScore = dto.MaxScore.Value;

            var updated = await _repo.UpdateAsync(entity);
            return MapToDto(updated);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            return await _repo.DeleteAsync(id);
        }

        // ── MAPPER ────────────────────────────────────────

        private static AssessmentDto MapToDto(Assessment a) => new()
        {
            AssessmentId = a.AssessmentId,
            Name = a.Name,
            Description = a.Description,
            TotalQuestions = a.TotalQuestions,
            MaxScore = a.MaxScore
        };
    }
}
