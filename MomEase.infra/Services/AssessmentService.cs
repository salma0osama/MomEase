using Microsoft.AspNetCore.Http;
using MomEase.core.DTOS.AssessmentDto;
using MomEase.core.Entities;
using MomEase.core.Interfaces;
using MomEase.infra.Helpers;
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
        private readonly IHttpContextAccessor _httpContextAccessor;
        public AssessmentService(
            IAssessmentRepository repo,
            IHttpContextAccessor httpContextAccessor)
        {
            _repo = repo;
            _httpContextAccessor = httpContextAccessor;
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
                NameAr = dto.NameAr,  // ⬅️
                Description = dto.Description,
                DescriptionAr = dto.DescriptionAr,  // ⬅️
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

            if (dto.Name != null) entity.Name = dto.Name;
            if (dto.NameAr != null) entity.NameAr = dto.NameAr;  // ⬅️
            if (dto.Description != null) entity.Description = dto.Description;
            if (dto.DescriptionAr != null) entity.DescriptionAr = dto.DescriptionAr;  // ⬅️
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

        private AssessmentDto MapToDto(Assessment a)
        {
            var lang = LanguageHelper.GetLang(_httpContextAccessor);

            return new AssessmentDto
            {
                AssessmentId = a.AssessmentId,
                Name = LanguageHelper.GetLocalized(a.NameAr, a.Name, lang),
                Description = LanguageHelper.GetLocalized(a.DescriptionAr, a.Description, lang),
                TotalQuestions = a.TotalQuestions,
                MaxScore = a.MaxScore
            };
        }
    }
}
