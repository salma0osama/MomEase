using Microsoft.AspNetCore.Http;
using MomEase.core.DTOS.AssessmentDto;
using MomEase.core.Entities;
using MomEase.core.Interfaces;
using MomEase.infra.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MomEase.infra.Services
{
    public class ScoreLevelService : IScoreLevelService
    {
        private readonly IScoreLevelRepository _repo;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ScoreLevelService(IScoreLevelRepository repo,
    IHttpContextAccessor httpContextAccessor)
        {
            _repo = repo;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<IEnumerable<ScoreLevelDto>> GetAllByAssessmentAsync(int assessmentId)
        {
            var levels = await _repo.GetAllByAssessmentAsync(assessmentId);
            return levels.Select(MapToDto);
        }

        public async Task<ScoreLevelDto?> GetByIdAsync(int assessmentId, int levelId)
        {
            var level = await _repo.GetByIdAsync(assessmentId, levelId);
            if (level == null) return null;
            return MapToDto(level);
        }

        public async Task<(ScoreLevelDto? result, string? error)> CreateAsync(int assessmentId, CreateScoreLevelDto dto)
        {
            if (!await _repo.AssessmentExistsAsync(assessmentId))
                return (null, $"Assessment {assessmentId} not found.");

            var entity = new ScoreLevel
            {
                AssessmentId = assessmentId,
                MinScore = dto.MinScore,
                MaxScore = dto.MaxScore,
                LevelName = dto.LevelName,
                Advice = dto.Advice
            };

            var created = await _repo.CreateAsync(entity);
            return (MapToDto(created), null);
        }

        public async Task<(ScoreLevelDto? result, string? error)> UpdateAsync(int assessmentId, int levelId, UpdateScoreLevelDto dto)
        {
            var entity = await _repo.GetByIdAsync(assessmentId, levelId);
            if (entity == null)
                return (null, $"Score level {levelId} not found in Assessment {assessmentId}.");

            if (dto.MinScore != null) entity.MinScore = dto.MinScore.Value;
            if (dto.MaxScore != null) entity.MaxScore = dto.MaxScore.Value;
            if (dto.LevelName != null) entity.LevelName = dto.LevelName;
            if (dto.Advice != null) entity.Advice = dto.Advice;

            var updated = await _repo.UpdateAsync(entity);
            return (MapToDto(updated), null);
        }

        public async Task<bool> DeleteAsync(int assessmentId, int levelId)
        {
            return await _repo.DeleteAsync(assessmentId, levelId);
        }

        // ── MAPPER ────────────────────────────────────────
        private ScoreLevelDto MapToDto(ScoreLevel sl)
        {
            var lang = LanguageHelper.GetLang(_httpContextAccessor);

            return new ScoreLevelDto
            {
                LevelId = sl.LevelId,
                AssessmentId = sl.AssessmentId,
                MinScore = sl.MinScore,
                MaxScore = sl.MaxScore,
                LevelName = LanguageHelper.GetLocalized(sl.LevelNameAr, sl.LevelName, lang),
                Advice = LanguageHelper.GetLocalized(sl.AdviceAr, sl.Advice, lang)
            };
        }
    }
}