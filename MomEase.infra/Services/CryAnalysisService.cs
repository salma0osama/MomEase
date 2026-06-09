using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using MomEase.core.DTOS.CryAnalysisDto;
using MomEase.core.Entities;
using MomEase.core.Interfaces;
using MomEase.infra.Helpers;
using MomEase.infra.Repositories;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MomEase.infra.Services
{
    public class CryAnalysisService : ICryAnalysisService
    {
        private readonly ICryAnalysisRepository _analysisRepo;
        private readonly ICryReasonsRepository _reasonsRepo;
        private readonly ICryAnalysisAIService _aiService;
        private readonly IChildRepository _childRepo;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<CryAnalysisService> _logger;

        // ترجمة أسباب البكاء للعربي
        private static readonly Dictionary<string, string> ReasonNamesAr = new()
        {
            { "belly_pain",  "ألم في البطن" },
            { "burping",     "تجشؤ" },
            { "discomfort",  "انزعاج" },
            { "hungry",      "جوع" },
            { "laugh",       "ضحك" }
        };

        public CryAnalysisService(
            ICryAnalysisRepository analysisRepo,
            ICryReasonsRepository reasonsRepo,
            ICryAnalysisAIService aiService,
            IChildRepository childRepo,
            IHttpContextAccessor httpContextAccessor,
            ILogger<CryAnalysisService> logger)
        {
            _analysisRepo = analysisRepo;
            _reasonsRepo = reasonsRepo;
            _aiService = aiService;
            _childRepo = childRepo;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public async Task<CryAnalysisResponseDto> AnalyzeNewAudioAsync(
            int userId,
            CryAnalysisRequestDto request,
            IFormFile audio)
        {
            try
            {
                if (audio == null || audio.Length == 0)
                    throw new ArgumentException("No audio file provided");

                // التحقق من الـ ChildId لو موجود
                if (request.ChildId.HasValue)
                {
                    var isOwned = await _childRepo.IsChildOwnedByUserAsync(request.ChildId.Value, userId);
                    if (!isOwned)
                        throw new KeyNotFoundException("Child not found or does not belong to this user");
                }

                // حفظ الـ audio
                var audioUrl = await SaveAudioAsync(audio);

                // تحليل الـ AI
                var (prediction, confidence, allScores) = await _aiService.AnalyzeAudioAsync(audio);

                // جلب الـ CryReason من الـ DB
                var reason = await _reasonsRepo.GetByNameAsync(prediction);

                // حفظ في الـ DB
                var analysis = new CryAnalyses
                {
                    UserId = userId,
                    ChildId = request.ChildId,
                    AudioUrl = audioUrl,
                    Result = prediction.ToString(),
                    Confidence = confidence,
                    CryreasonId = reason?.CryreasonId,
                    CreatedAt = DateTime.Now.AddHours(1)
                };

                var saved = await _analysisRepo.AddAsync(analysis);
                _logger.LogInformation("✅ Cry analysis saved: {Id}", saved.CryId);

                return MapToDto(saved, reason, allScores);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error analyzing audio");
                throw;
            }
        }

        public async Task<List<CryAnalysisResponseDto>> GetUserAnalysesAsync(int userId)
        {
            try
            {
                var analyses = await _analysisRepo.GetByUserIdAsync(userId);
                return analyses.Select(a => MapToDto(a, a.CryReason)).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error getting analyses for user {UserId}", userId);
                throw;
            }
        }

        public async Task<List<CryAnalysisResponseDto>> GetChildAnalysesAsync(int childId)
        {
            try
            {
                var analyses = await _analysisRepo.GetByChildIdAsync(childId);
                return analyses.Select(a => MapToDto(a, a.CryReason)).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error getting analyses for child {ChildId}", childId);
                throw;
            }
        }

        public async Task<CryAnalysisResponseDto> GetAnalysisByIdAsync(int id)
        {
            try
            {
                var analysis = await _analysisRepo.GetByIdAsync(id);
                if (analysis == null)
                    throw new KeyNotFoundException($"Cry analysis with ID {id} not found");

                return MapToDto(analysis, analysis.CryReason);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error getting analysis {Id}", id);
                throw;
            }
        }

        public async Task DeleteAnalysisAsync(int id)
        {
            try
            {
                var analysis = await _analysisRepo.GetByIdAsync(id);
                if (analysis == null)
                    throw new KeyNotFoundException($"Cry analysis with ID {id} not found");

                // حذف الـ audio file من الـ server
                DeleteAudioFile(analysis.AudioUrl);

                await _analysisRepo.DeleteAsync(id);
                _logger.LogInformation("✅ Cry analysis deleted: {Id}", id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error deleting analysis {Id}", id);
                throw;
            }
        }

        // ===== Private Helpers =====

        private CryAnalysisResponseDto MapToDto(
            CryAnalyses a,
            CryReasons reason,
            Dictionary<string, double> allScores = null)
        {
            var lang = LanguageHelper.GetLang(_httpContextAccessor);
            var resultKey = a.Result?.ToLower() ?? "";

            var displayResult = lang.StartsWith("ar") && ReasonNamesAr.ContainsKey(resultKey)
                ? ReasonNamesAr[resultKey]
                : FormatResultForDisplay(a.Result);

            var advice = reason != null
                ? LanguageHelper.GetLocalized(reason.AdviceAr, reason.Advice, lang)
                : null;

            return new CryAnalysisResponseDto
            {
                CryId = a.CryId,
                AudioUrl = a.AudioUrl,
                Result = displayResult,
                Confidence = a.Confidence,
                Advice = advice,
                ChildId = a.ChildId,
                CreatedAt = a.CreatedAt,
                AllScores = allScores
            };
        }

        private string FormatResultForDisplay(string result)
        {
            if (string.IsNullOrEmpty(result)) return result;
            // belly_pain → Belly Pain
            return System.Globalization.CultureInfo.CurrentCulture.TextInfo
                .ToTitleCase(result.Replace("_", " "));
        }

        private async Task<string> SaveAudioAsync(IFormFile audio)
        {
            var uploadsFolder = Path.Combine("wwwroot", "uploads", "cry-analysis");

            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(audio.FileName)}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            using var stream = new FileStream(filePath, FileMode.Create);
            await audio.CopyToAsync(stream);

            return $"/uploads/cry-analysis/{fileName}";
        }

        private void DeleteAudioFile(string audioUrl)
        {
            try
            {
                if (string.IsNullOrEmpty(audioUrl)) return;
                var filePath = Path.Combine("wwwroot", audioUrl.TrimStart('/'));
                if (File.Exists(filePath))
                    File.Delete(filePath);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "⚠️ Could not delete audio file: {Url}", audioUrl);
            }
        }
    }
}