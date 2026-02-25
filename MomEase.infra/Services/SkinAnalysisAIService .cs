using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MomEase.core.Enums;
using MomEase.core.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;

namespace MomEase.infra.Services
{
    public class SkinAnalysisAIService : ISkinAnalysisAIService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _apiUrl;
        private readonly ILogger<SkinAnalysisAIService> _logger;

        public SkinAnalysisAIService(
            IHttpClientFactory httpClientFactory,
            IConfiguration config,
            ILogger<SkinAnalysisAIService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _apiUrl = config["AI:ApiUrl"] ?? "https://sohailaaaz-skin-disease-api.hf.space";
            _logger = logger;
        }

        public async Task<(SkinAnalysisDiseaseName disease, double confidence)> AnalyzeImageAsync(IFormFile image)
        {
            try
            {
                _logger.LogInformation("🔍 Analyzing: {FileName}", image.FileName);

                ValidateImage(image);

                var client = _httpClientFactory.CreateClient();
                client.Timeout = TimeSpan.FromMinutes(2);

                using var content = new MultipartFormDataContent();
                using var stream = image.OpenReadStream();
                using var streamContent = new StreamContent(stream);

                streamContent.Headers.ContentType = new MediaTypeHeaderValue(image.ContentType ?? "image/jpeg");
                content.Add(streamContent, "file", image.FileName);

                var url = $"{_apiUrl}/predict";
                _logger.LogInformation("📤 POST {Url}", url);

                var response = await client.PostAsync(url, content);

                _logger.LogInformation("📥 Status: {Status}", response.StatusCode);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    _logger.LogError("❌ Error: {Error}", error);
                    throw new Exception($"API Error: {response.StatusCode}");
                }

                var json = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("✅ Response: {Json}", json);

                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                if (!root.GetProperty("success").GetBoolean())
                {
                    throw new Exception("Prediction failed");
                }

                // ✅ استخراج المرض والثقة
                var disease = root.GetProperty("disease").GetString();
                var confidence = root.GetProperty("confidence").GetDouble();

                _logger.LogInformation("🎯 Disease: {Disease}, Confidence: {Confidence}%", disease, confidence);

                // ✅ رجوع tuple
                return (ParseDiseaseNameToEnum(disease), confidence);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Analysis failed");
                throw new Exception($"فشل تحليل الصورة: {ex.Message}", ex);
            }
        }

        private void ValidateImage(IFormFile image)
        {
            if (image == null || image.Length == 0)
                throw new ArgumentException("الصورة فارغة");

            var ext = Path.GetExtension(image.FileName).ToLowerInvariant();
            if (!new[] { ".jpg", ".jpeg", ".png", ".webp", ".bmp" }.Contains(ext))
                throw new ArgumentException($"نوع ملف غير مدعوم: {ext}");

            if (image.Length > 10 * 1024 * 1024)
                throw new ArgumentException("حجم الملف كبير جداً (أكثر من 10MB)");

            _logger.LogInformation("✅ Validation passed: {FileName} ({Size} KB)",
                image.FileName, image.Length / 1024);
        }

        private SkinAnalysisDiseaseName ParseDiseaseNameToEnum(string diseaseName)
        {
            if (string.IsNullOrWhiteSpace(diseaseName))
                throw new ArgumentException("اسم المرض فارغ");

            diseaseName = diseaseName
                .Trim()
                .Replace("-", "")
                .Replace("_", "")
                .Replace(" ", "");

            _logger.LogInformation("🔄 Parsing: '{DiseaseName}'", diseaseName);

            if (Enum.TryParse<SkinAnalysisDiseaseName>(diseaseName, true, out var result))
            {
                _logger.LogInformation("✅ Matched: {Enum}", result);
                return result;
            }

            var mapping = new Dictionary<string, SkinAnalysisDiseaseName>(StringComparer.OrdinalIgnoreCase)
            {
                { "Chickenpox", SkinAnalysisDiseaseName.Chikenpox },
                { "Diaper", SkinAnalysisDiseaseName.Diaper },
                { "HandFootAndMouth", SkinAnalysisDiseaseName.HandFootAndMouth },
                { "Hand-Food-And-Mouth", SkinAnalysisDiseaseName.HandFootAndMouth },
                { "HandFoodAndMouth", SkinAnalysisDiseaseName.HandFootAndMouth },
                { "Impetigo", SkinAnalysisDiseaseName.Impetigo },
                { "InsectBites", SkinAnalysisDiseaseName.InsectBites },
                { "Insect_bites", SkinAnalysisDiseaseName.InsectBites }
            };

            if (mapping.TryGetValue(diseaseName, out var mapped))
            {
                _logger.LogInformation("✅ Mapped: '{Original}' → {Enum}", diseaseName, mapped);
                return mapped;
            }

            _logger.LogWarning("⚠️ Unknown disease '{DiseaseName}', defaulting to InsectBites", diseaseName);
            return SkinAnalysisDiseaseName.InsectBites;
        }
    }
}