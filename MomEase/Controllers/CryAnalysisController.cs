using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MomEase.core.DTOS.CryAnalysisDto;
using MomEase.core.Interfaces;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MomEase.api.Controllers
{
    [ApiController]
    [Authorize]
    public class CryAnalysisController : ControllerBase
    {
        private readonly ICryAnalysisService _analysisService;
        private readonly ICryReasonsService _reasonsService;

        public CryAnalysisController(
            ICryAnalysisService analysisService,
            ICryReasonsService reasonsService)
        {
            _analysisService = analysisService;
            _reasonsService = reasonsService;
        }

        private int GetUserId() =>
            int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

        // =============================================
        // CryAnalysis Endpoints
        // =============================================

        ///// <summary>تحليل صوت بكاء جديد - ChildId اختياري</summary>
        //[HttpPost("api/cry-analysis")]
        //public async Task<IActionResult> AnalyzeCry(
        //    [FromForm] CryAnalysisRequestDto request,
        //    IFormFile audio)
        //{
        //    try
        //    {
        //        if (audio == null || audio.Length == 0)
        //            return BadRequest(new { message = "Please provide an audio file" });

        //        var result = await _analysisService.AnalyzeNewAudioAsync(GetUserId(), request, audio);
        //        return Ok(result);
        //    }
        //    catch (ArgumentException ex)
        //    {
        //        return BadRequest(new { message = ex.Message });
        //    }
        //    catch (KeyNotFoundException ex)
        //    {
        //        return NotFound(new { message = ex.Message });
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, new { message = "Analysis failed", detail = ex.Message });
        //    }
        //}

        /// <summary>الحصول على جميع تحليلات المستخدم الحالي</summary>
        [HttpGet("api/cry-analysis")]
        public async Task<IActionResult> GetMyAnalyses()
        {
            try
            {
                var results = await _analysisService.GetUserAnalysesAsync(GetUserId());
                return Ok(results);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>الحصول على تحليل معين</summary>
        [HttpGet("api/cry-analysis/{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var result = await _analysisService.GetAnalysisByIdAsync(id);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>حذف تحليل</summary>
        [HttpDelete("api/cry-analysis/{id}")]
        public async Task<IActionResult> DeleteAnalysis(int id)
        {
            try
            {
                await _analysisService.DeleteAnalysisAsync(id);
                return Ok(new { message = "Analysis deleted successfully" });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>الحصول على تحليلات طفل معين</summary>
        [HttpGet("api/children/{childId}/cry-analysis")]
        public async Task<IActionResult> GetChildAnalyses(int childId)
        {
            try
            {
                var results = await _analysisService.GetChildAnalysesAsync(childId);
                return Ok(results);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>إضافة تحليل لطفل معين مباشرة</summary>
        [HttpPost("api/children/{childId}/cry-analysis")]
        public async Task<IActionResult> AnalyzeForChild(int childId, IFormFile audio)
        {
            try
            {
                if (audio == null || audio.Length == 0)
                    return BadRequest(new { message = "Please provide an audio file" });

                var request = new CryAnalysisRequestDto { ChildId = childId };
                var result = await _analysisService.AnalyzeNewAudioAsync(GetUserId(), request, audio);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Analysis failed", detail = ex.Message });
            }
        }

        // =============================================
        // CryReasons Endpoints
        // =============================================

        /// <summary>الحصول على جميع أسباب البكاء</summary>
        [HttpGet("api/cry-reasons")]
        public async Task<IActionResult> GetAllReasons()
        {
            try
            {
                var reasons = await _reasonsService.GetAllAsync();
                return Ok(new { success = true, data = reasons });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>الحصول على سبب معين</summary>
        [HttpGet("api/cry-reasons/{id}")]
        public async Task<IActionResult> GetReasonById(int id)
        {
            try
            {
                var reason = await _reasonsService.GetByIdAsync(id);
                return Ok(new { success = true, data = reason });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
    }
}