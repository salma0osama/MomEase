using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MomEase.core.Enums;
using MomEase.core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MomEase.infra.Services
{
    /// <summary>
    /// حل عملي: يجرب Gradio الحقيقي، ولو فشل يستخدم TensorFlow.js model مباشرة
    /// </summary>
    public class SkinAnalysisAIService : ISkinAnalysisAIService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _fastApiUrl;
        private readonly ILogger<SkinAnalysisAIService> _logger;

        public SkinAnalysisAIService(
            IHttpClientFactory httpClientFactory,
            IConfiguration config,
            ILogger<SkinAnalysisAIService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _fastApiUrl = config["AI:FastAPIUrl"] ?? "http://localhost:7860";
            _logger = logger;
        }

        public async Task<SkinAnalysisDiseaseName> AnalyzeImageAsync(IFormFile image)
        {
            try
            {
                _logger.LogInformation("🔍 Analyzing image: {FileName}", image.FileName);

                // Validate
                ValidateImage(image);

                // Call FastAPI
                var client = _httpClientFactory.CreateClient();
                client.Timeout = TimeSpan.FromMinutes(2);

                using var content = new MultipartFormDataContent();
                using var stream = image.OpenReadStream();
                using var streamContent = new StreamContent(stream);

                streamContent.Headers.ContentType = new MediaTypeHeaderValue(image.ContentType ?? "image/jpeg");
                content.Add(streamContent, "file", image.FileName);

                var url = $"{_fastApiUrl}/predict";
                _logger.LogInformation("📤 POST {Url}", url);

                var response = await client.PostAsync(url, content);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    throw new Exception($"FastAPI Error ({response.StatusCode}): {error}");
                }

                var responseBody = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("✅ Response: {Response}", responseBody);

                // Parse response
                using var doc = JsonDocument.Parse(responseBody);
                var root = doc.RootElement;

                var disease = root.GetProperty("disease").GetString();

                _logger.LogInformation("🎯 Predicted: {Disease}", disease);

                return ParseDiseaseNameToEnum(disease);
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
            if (!new[] { ".jpg", ".jpeg", ".png", ".webp" }.Contains(ext))
                throw new ArgumentException($"نوع ملف غير مدعوم: {ext}");

            if (image.Length > 10 * 1024 * 1024)
                throw new ArgumentException("حجم الملف كبير جداً (أكثر من 10MB)");
        }

        private SkinAnalysisDiseaseName ParseDiseaseNameToEnum(string diseaseName)
        {
            if (string.IsNullOrWhiteSpace(diseaseName))
                throw new ArgumentException("اسم المرض فارغ");

            // Direct enum parsing
            if (Enum.TryParse<SkinAnalysisDiseaseName>(diseaseName, true, out var result))
            {
                _logger.LogInformation("✅ Mapped to enum: {Enum}", result);
                return result;
            }

            // Fallback mapping
            var mapping = new System.Collections.Generic.Dictionary<string, SkinAnalysisDiseaseName>(StringComparer.OrdinalIgnoreCase)
            {
                { "HandFootAndMouth", SkinAnalysisDiseaseName.HandFootAndMouth },
                { "Chickenpox", SkinAnalysisDiseaseName.Chikenpox },
                { "Impetigo", SkinAnalysisDiseaseName.Impetigo },
                { "InsectBites", SkinAnalysisDiseaseName.InsectBites },
                { "Insect_bites", SkinAnalysisDiseaseName.InsectBites },
                { "Diaper", SkinAnalysisDiseaseName.Diaper },
            };

            if (mapping.TryGetValue(diseaseName, out var mapped))
            {
                _logger.LogInformation("✅ Mapped '{Original}' → {Enum}", diseaseName, mapped);
                return mapped;
            }

            _logger.LogWarning("⚠️ Unknown disease '{Disease}', defaulting to InsectBites", diseaseName);
            return SkinAnalysisDiseaseName.InsectBites;
        }
    }
}
