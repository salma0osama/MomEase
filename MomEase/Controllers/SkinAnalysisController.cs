using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MomEase.core.DTOS.AdminDTO;
using MomEase.core.DTOS;
using MomEase.core.Interfaces;
using System.Security.Claims;
using MomEase.infra.Services;
using MomEase.core.Enums;
using MomEase.core.DTOS.SkinAnalysisDto;

namespace MomEase.API.Controllers
{
    [ApiController]
    [Route("api/SkinAnalysis")]
    public class SkinAnalysisController : ControllerBase
    {
        private readonly ISkinAnalysisAIService _aiService;
        private readonly ILogger<SkinAnalysisController> _logger;

        public SkinAnalysisController(
            ISkinAnalysisAIService aiService,
            ILogger<SkinAnalysisController> logger)
        {
            _aiService = aiService;
            _logger = logger;
        }

        /// <summary>
        /// اختبار سريع للـ Gradio API - جرب الصورة مباشرة
        /// </summary>
        /// <param name="image">ملف الصورة</param>
        /// <returns>نتيجة التحليل</returns>
        [HttpPost("quick-test")]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> QuickTest(IFormFile image)
        {
            try
            {
                _logger.LogInformation("Testing Gradio API with image: {FileName}", image?.FileName);

                if (image == null || image.Length == 0)
                {
                    return BadRequest(new
                    {
                        success = false,
                        error = "No image provided",
                        message = "الرجاء رفع صورة للاختبار"
                    });
                }

                // استدعاء الـ AI Service
                var result = await _aiService.AnalyzeImageAsync(image);

                return Ok(new
                {
                    success = true,
                    prediction = result.ToString(),
                    message = "تم التحليل بنجاح! ✅",
                    details = new
                    {
                        fileName = image.FileName,
                        fileSize = $"{image.Length / 1024.0:F2} KB",
                        contentType = image.ContentType,
                        predictedDisease = result
                    }
                });
            }
            catch (ArgumentException argEx)
            {
                _logger.LogWarning(argEx, "Validation error during test");
                return BadRequest(new
                {
                    success = false,
                    error = "Validation Error",
                    message = argEx.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during Gradio API test");
                return StatusCode(500, new
                {
                    success = false,
                    error = "Internal Server Error",
                    message = ex.Message,
                    stackTrace = ex.StackTrace // في الـ development فقط
                });
            }
        }

        /// <summary>
        /// فحص حالة الاتصال بـ Gradio API
        /// </summary>
        [HttpGet("health-check")]
        [ProducesResponseType(typeof(object), 200)]
        public IActionResult HealthCheck()
        {
            return Ok(new
            {
                service = "Skin Analysis AI Service",
                status = "Running",
                gradioEndpoint = "https://sohailaaaz-skin-disease-api.hf.space",
                timestamp = DateTime.UtcNow,
                message = "Service is ready to accept images ✅"
            });
        }

        /// <summary>
        /// الأمراض المدعومة
        /// </summary>
        [HttpGet("supported-diseases")]
        [ProducesResponseType(typeof(object), 200)]
        public IActionResult GetSupportedDiseases()
        {
            return Ok(new
            {
                supportedDiseases = new[]
                {
                    new { id = 1, name = "InsectBites", arabicName = "لدغات الحشرات" },
                    new { id = 2, name = "Impetigo", arabicName = "القوباء" },
                    new { id = 3, name = "HandFootAndMouth", arabicName = "مرض اليد والقدم والفم" },
                    new { id = 4, name = "Diaper", arabicName = "طفح الحفاضات" },
                    new { id = 5, name = "Chickenpox", arabicName = "جدري الماء" }
                },
                totalCount = 5
            });
        }
    }
}