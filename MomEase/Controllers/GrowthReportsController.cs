using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MomEase.core.DTOS.GrowthReport;
using MomEase.core.Interfaces;
using System.Security.Claims;

namespace MomEase.api.Controllers
{
    [Route("api/children/{childId}/growth-reports")]
    [ApiController]
    [Authorize]
    public class GrowthReportsController : ControllerBase
    {
        private readonly IGrowthReportService _service;
        private readonly IChildRepository _childRepo;
        private readonly ILogger<GrowthReportsController> _logger;

        public GrowthReportsController(
            IGrowthReportService service,
            IChildRepository childRepo,
            ILogger<GrowthReportsController> logger)
        {
            _service = service;
            _childRepo = childRepo;
            _logger = logger;
        }

        /// <summary>
        /// إنشاء تقرير نمو جديد
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> GenerateReport(
            int childId,
            [FromBody] CreateGrowthReportDto dto)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var report = await _service.GenerateReportAsync(childId, userId, dto);

                return CreatedAtAction(
                    nameof(GetReport),
                    new { childId, reportId = report.ReportId },
                    new { success = true, message = "Report generated successfully", data = report }
                );
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating report");
                return StatusCode(500, new { success = false, message = "Failed to generate report" });
            }
        }

        /// <summary>
        /// الحصول على جميع التقارير للطفل
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAllReports(int childId)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var reports = await _service.GetAllReportsAsync(childId, userId);

                return Ok(new { success = true, data = reports });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving reports");
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// الحصول على تقرير معين
        /// </summary>
        [HttpGet("{reportId}")]
        public async Task<IActionResult> GetReport(int childId, int reportId)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var report = await _service.GetReportByIdAsync(reportId, childId, userId);

                return Ok(new { success = true, data = report });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving report");
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// الحصول على أحدث تقرير
        /// </summary>
        [HttpGet("latest")]
        public async Task<IActionResult> GetLatestReport(int childId)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var report = await _service.GetLatestReportAsync(childId, userId);

                if (report == null)
                    return NotFound(new { success = false, message = "No reports found" });

                return Ok(new { success = true, data = report });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving latest report");
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// حذف تقرير
        /// </summary>
        [HttpDelete("{reportId}")]
        public async Task<IActionResult> DeleteReport(int childId, int reportId)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                await _service.DeleteReportAsync(reportId, childId, userId);

                return Ok(new { success = true, message = "Report deleted successfully" });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting report");
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }
}
