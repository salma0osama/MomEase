using Microsoft.AspNetCore.Http;
using MomEase.core.DTOS.SkinAnalysisDto;
using MomEase.core.Entities;
using MomEase.core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MomEase.infra.Services
{
    public class SkinAnalysisService : ISkinAnalysisService
    {
        private readonly ISkinAnalysisRepository _analysisRepo;
        private readonly IDiseaseRepository _diseaseRepo;
        private readonly ISkinAnalysisAIService _aiService;

        public SkinAnalysisService(
            ISkinAnalysisRepository analysisRepo,
            IDiseaseRepository diseaseRepo,
            ISkinAnalysisAIService aiService)
        {
            _analysisRepo = analysisRepo;
            _diseaseRepo = diseaseRepo;
            _aiService = aiService;
        }

        public async Task<SkinAnalysisResponseDto> AnalyzeNewImageAsync(
    SkinAnalysisRequestDto request,
    IFormFile image)
        {
            // Validate
            if (image == null || image.Length == 0)
                throw new Exception("No image provided");

            // Save image
            var imageUrl = await SaveImageAsync(image);

            // ✅ Call AI with tuple
            var (predictedDisease, confidence) = await _aiService.AnalyzeImageAsync(image);

            // Get disease from DB
            var disease = await _diseaseRepo.GetByNameAsync(predictedDisease);

            // Save to DB
            var analysis = new SkinAnalyses
            {
                UserId = request.UserId,
                ChildId = request.ChildId,
                ImageUrl = imageUrl,
                Result = predictedDisease.ToString(),
                DiseaseId = disease?.DiseaseId,
                CreatedAt = DateTime.Now
            };

            var saved = await _analysisRepo.AddAsync(analysis);

            return new SkinAnalysisResponseDto
            {
                SkinanalysisId = saved.SkinanalysisId,
                ImageUrl = saved.ImageUrl,
                Result = saved.Result,
                DiseaseName = disease?.Name.ToString(),
                Advice = disease?.Advice,
                Confidence = confidence,  // ✅ أضفنا الثقة
                CreatedAt = saved.CreatedAt
            };
        }

        public async Task<List<SkinAnalysisResponseDto>> GetUserAnalysesAsync(int userId)
        {
            var analyses = await _analysisRepo.GetByUserIdAsync(userId);

            return analyses.Select(a => new SkinAnalysisResponseDto
            {
                SkinanalysisId = a.SkinanalysisId,
                ImageUrl = a.ImageUrl,
                Result = a.Result,
                DiseaseName = a.Disease?.Name.ToString(),
                Advice = a.Disease?.Advice,
                CreatedAt = a.CreatedAt
            }).ToList();
        }

        public async Task<List<SkinAnalysisResponseDto>> GetChildAnalysesAsync(int childId)
        {
            var analyses = await _analysisRepo.GetByChildIdAsync(childId);

            return analyses.Select(a => new SkinAnalysisResponseDto
            {
                SkinanalysisId = a.SkinanalysisId,
                ImageUrl = a.ImageUrl,
                Result = a.Result,
                DiseaseName = a.Disease?.Name.ToString(),
                Advice = a.Disease?.Advice,
                CreatedAt = a.CreatedAt
            }).ToList();
        }

        public async Task<SkinAnalysisResponseDto> GetAnalysisByIdAsync(int id)
        {
            var analysis = await _analysisRepo.GetByIdAsync(id);

            if (analysis == null)
                throw new Exception("Analysis not found");

            return new SkinAnalysisResponseDto
            {
                SkinanalysisId = analysis.SkinanalysisId,
                ImageUrl = analysis.ImageUrl,
                Result = analysis.Result,
                DiseaseName = analysis.Disease?.Name.ToString(),
                Advice = analysis.Disease?.Advice,
                CreatedAt = analysis.CreatedAt
            };
        }

        public async Task DeleteAnalysisAsync(int id)
        {
            await _analysisRepo.DeleteAsync(id);
        }

        private async Task<string> SaveImageAsync(IFormFile image)
        {
            var uploadsFolder = Path.Combine("wwwroot", "uploads", "skin-analysis");

            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(image.FileName)}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await image.CopyToAsync(stream);
            }

            return $"/uploads/skin-analysis/{fileName}";
        }
    }
}
