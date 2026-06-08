using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using MomEase.core.DTOS.CryAnalysisDto;
using MomEase.core.Entities;
using MomEase.core.Interfaces;
using MomEase.infra.Helpers;
using MomEase.infra.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MomEase.infra.Services
{
    public class CryReasonsService : ICryReasonsService
    {
        private readonly ICryReasonsRepository _reasonsRepo;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<CryReasonsService> _logger;

        public CryReasonsService(
            ICryReasonsRepository reasonsRepo,
            IHttpContextAccessor httpContextAccessor,
            ILogger<CryReasonsService> logger)
        {
            _reasonsRepo = reasonsRepo;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public async Task<List<CryReasonResponseDto>> GetAllAsync()
        {
            try
            {
                var reasons = await _reasonsRepo.GetAllAsync();
                var lang = LanguageHelper.GetLang(_httpContextAccessor);
                return reasons.Select(r => MapToDto(r, lang)).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error getting all cry reasons");
                throw;
            }
        }

        public async Task<CryReasonResponseDto> GetByIdAsync(int id)
        {
            try
            {
                var reason = await _reasonsRepo.GetByIdAsync(id);
                if (reason == null)
                    throw new KeyNotFoundException($"Cry reason with ID {id} not found");

                var lang = LanguageHelper.GetLang(_httpContextAccessor);
                return MapToDto(reason, lang);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error getting cry reason {Id}", id);
                throw;
            }
        }

        public async Task<CryReasonResponseDto> CreateAsync(CryReasonCreateDto dto)
        {
            try
            {
                var reason = new CryReasons
                {
                    Name = dto.Name,
                    NameAr = dto.NameAr,
                    Advice = dto.Advice,
                    AdviceAr = dto.AdviceAr
                };

                var saved = await _reasonsRepo.AddAsync(reason);
                _logger.LogInformation("✅ Cry reason created: {Id}", saved.CryreasonId);

                var lang = LanguageHelper.GetLang(_httpContextAccessor);
                return MapToDto(saved, lang);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error creating cry reason");
                throw;
            }
        }

        public async Task<CryReasonResponseDto> UpdateAsync(int id, CryReasonUpdateDto dto)
        {
            try
            {
                var reason = await _reasonsRepo.GetByIdAsync(id);
                if (reason == null)
                    throw new KeyNotFoundException($"Cry reason with ID {id} not found");

                if (dto.Name != null) reason.Name = dto.Name;
                if (dto.NameAr != null) reason.NameAr = dto.NameAr;
                if (dto.Advice != null) reason.Advice = dto.Advice;
                if (dto.AdviceAr != null) reason.AdviceAr = dto.AdviceAr;

                var updated = await _reasonsRepo.UpdateAsync(reason);
                _logger.LogInformation("✅ Cry reason updated: {Id}", id);

                var lang = LanguageHelper.GetLang(_httpContextAccessor);
                return MapToDto(updated, lang);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error updating cry reason {Id}", id);
                throw;
            }
        }

        public async Task DeleteAsync(int id)
        {
            try
            {
                var reason = await _reasonsRepo.GetByIdAsync(id);
                if (reason == null)
                    throw new KeyNotFoundException($"Cry reason with ID {id} not found");

                await _reasonsRepo.DeleteAsync(id);
                _logger.LogInformation("✅ Cry reason deleted: {Id}", id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error deleting cry reason {Id}", id);
                throw;
            }
        }

        private CryReasonResponseDto MapToDto(CryReasons r, string lang)
        {
            return new CryReasonResponseDto
            {
                CryreasonId = r.CryreasonId,
                Name = LanguageHelper.GetLocalized(r.NameAr, r.Name, lang),
                Advice = LanguageHelper.GetLocalized(r.AdviceAr, r.Advice, lang)
            };
        }
    }
}