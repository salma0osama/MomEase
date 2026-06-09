using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using MomEase.core.DTOS.SkinAnalysisDto;
using MomEase.core.Entities;
using MomEase.core.Interfaces;
using MomEase.infra.Helpers;

namespace MomEase.infra.Services
{
    public class SkinAnalysisService : ISkinAnalysisService
    {
        private readonly ISkinAnalysisRepository _analysisRepo;
        private readonly IDiseaseRepository _diseaseRepo;
        private readonly ISkinAnalysisAIService _aiService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<SkinAnalysisService> _logger;
        private readonly IChildRepository _childRepo;

        private static readonly Dictionary<string, string> DiseaseNamesAr = new()
        {
            { "Chikenpox", "جدري الماء" },
            { "Diaper", "طفح الحفاضات" },
            { "HandFootAndMouth", "مرض اليد والقدم والفم" },
            { "Impetigo", "القوباء" },
            { "InsectBites", "لدغات الحشرات" }
        };
        public SkinAnalysisService(
            ISkinAnalysisRepository analysisRepo,
            IDiseaseRepository diseaseRepo,
            ISkinAnalysisAIService aiService,
            IChildRepository childRepo,
            IHttpContextAccessor httpContextAccessor,
            ILogger<SkinAnalysisService> logger)
        {
            _analysisRepo = analysisRepo;
            _diseaseRepo = diseaseRepo;
            _aiService = aiService;
            _childRepo = childRepo;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }


        public async Task<SkinAnalysisResponseDto> AnalyzeNewImageAsync(
        int userId,
        SkinAnalysisRequestDto request,
        IFormFile image)
        {
            try
            {
                if (image == null || image.Length == 0)
                    throw new ArgumentException("No image provided");

                // تحقق من الـ ChildId لو موجود
                if (request.ChildId.HasValue)
                {
                    var isOwned = await _childRepo
                        .IsChildOwnedByUserAsync(request.ChildId.Value, userId);
                    if (!isOwned)
                        throw new KeyNotFoundException("Child not found");
                }

                var imageUrl = await SaveImageAsync(image);
                var (predictedDisease, confidence) = await _aiService.AnalyzeImageAsync(image);
                var disease = await _diseaseRepo.GetByNameAsync(predictedDisease);

                var analysis = new SkinAnalyses
                {
                    UserId = userId,
                    ChildId = request.ChildId,
                    ImageUrl = imageUrl,
                    Result = predictedDisease.ToString(),
                    DiseaseId = disease?.DiseaseId,
                    Confidence = confidence,
                    CreatedAt = DateTime.Now.AddHours(1)
                };

                var saved = await _analysisRepo.AddAsync(analysis);

                _logger.LogInformation("Skin analysis saved {Id}", saved.SkinanalysisId);

                return MapToDto(saved, disease, confidence);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing image");
                throw;
            }
        }

        public async Task<List<SkinAnalysisResponseDto>> GetUserAnalysesAsync(int userId)
        {
            try
            {
                var analyses = await _analysisRepo.GetByUserIdAsync(userId);
                return analyses.Select(a => MapToDto(a, a.Disease)).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving analyses for user {UserId}", userId);
                throw;
            }
        }

        public async Task<List<SkinAnalysisResponseDto>> GetChildAnalysesAsync(int childId)
        {
            try
            {
                var analyses = await _analysisRepo.GetByChildIdAsync(childId);
                return analyses.Select(a => MapToDto(a, a.Disease)).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving analyses for child {ChildId}", childId);
                throw;
            }
        }

        public async Task<SkinAnalysisResponseDto> GetAnalysisByIdAsync(int id)
        {
            try
            {
                var analysis = await _analysisRepo.GetByIdAsync(id);
                if (analysis == null)
                    throw new KeyNotFoundException("Analysis not found");

                return MapToDto(analysis, analysis.Disease);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving analysis {Id}", id);
                throw;
            }
        }

        public async Task DeleteAnalysisAsync(int id)
        {
            try
            {
                await _analysisRepo.DeleteAsync(id);
                _logger.LogInformation("Analysis deleted {Id}", id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting analysis {Id}", id);
                throw;
            }
        }

        public async Task<List<DiseaseDto>> GetAllDiseasesAsync()
        {
            try
            {
                var diseases = await _diseaseRepo.GetAllAsync();
                var lang = LanguageHelper.GetLang(_httpContextAccessor);

                return diseases.Select(d => new DiseaseDto
                {
                    DiseaseId = d.DiseaseId,
                    Name = lang.StartsWith("ar") && DiseaseNamesAr.ContainsKey(d.Name.ToString())
                        ? DiseaseNamesAr[d.Name.ToString()]
                        : d.Name.ToString(),
                    Advice = LanguageHelper.GetLocalized(d.AdviceAr, d.Advice, lang)
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all diseases");
                throw;
            }
        }

        public async Task<DiseaseDto> GetDiseaseByIdAsync(int id)
        {
            try
            {
                var disease = await _diseaseRepo.GetByIdAsync(id);
                if (disease == null)
                    throw new KeyNotFoundException("Disease not found");

                var lang = LanguageHelper.GetLang(_httpContextAccessor);

                return new DiseaseDto
                {
                    DiseaseId = disease.DiseaseId,
                    Name = lang.StartsWith("ar") && DiseaseNamesAr.ContainsKey(disease.Name.ToString())
                        ? DiseaseNamesAr[disease.Name.ToString()]
                        : disease.Name.ToString(),
                    Advice = LanguageHelper.GetLocalized(disease.AdviceAr, disease.Advice, lang)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving disease {Id}", id);
                throw;
            }
        }

        // ===== Private Helpers =====

        private SkinAnalysisResponseDto MapToDto(
            SkinAnalyses a,
            Diseases? disease,
            double? confidence = null)
        {
            var lang = LanguageHelper.GetLang(_httpContextAccessor);
            var diseaseName = disease?.Name.ToString() ?? a.Result;

            return new SkinAnalysisResponseDto
            {
                SkinanalysisId = a.SkinanalysisId,
                ImageUrl = a.ImageUrl,
                Result = lang.StartsWith("ar") && DiseaseNamesAr.ContainsKey(a.Result)
                   ? DiseaseNamesAr[a.Result]
                   : a.Result,
                DiseaseName = lang.StartsWith("ar") && DiseaseNamesAr.ContainsKey(diseaseName)
                    ? DiseaseNamesAr[diseaseName]
                    : diseaseName,
                Advice = LanguageHelper.GetLocalized(
                    disease?.AdviceAr, disease?.Advice ?? "", lang),
                Confidence = confidence ?? a.Confidence,
                CreatedAt = a.CreatedAt
            };
        }

        private async Task<string> SaveImageAsync(IFormFile image)
        {
            var uploadsFolder = Path.Combine("wwwroot", "uploads", "skin-analysis");

            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(image.FileName)}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            using var stream = new FileStream(filePath, FileMode.Create);
            await image.CopyToAsync(stream);

            return $"/uploads/skin-analysis/{fileName}";
        }
    }
}