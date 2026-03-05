using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MomEase.core.DTOS.CommunityDTO;
using MomEase.core.Interfaces;
using System.Security.Claims;

namespace MomEase.api.Controllers
{
    [ApiController]
    [Route("api/community")]
    [Authorize]
    public class PostReportsController : ControllerBase
    {
        private readonly ICommunityService _communityService;
        private readonly ILogger<PostReportsController> _logger;

        public PostReportsController(
            ICommunityService communityService,
            ILogger<PostReportsController> logger)
        {
            _communityService = communityService;
            _logger = logger;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                           ?? User.FindFirst("userId")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                throw new UnauthorizedAccessException("User ID not found");
            return userId;
        }

        /// <summary>Report a post</summary>
        [HttpPost("posts/{postId}/reports")]
        public async Task<ActionResult> ReportPost(int postId, [FromBody] CreateReportDto dto)
        {
            try
            {
                var userId = GetCurrentUserId();
                var report = await _communityService.ReportPostAsync(postId, userId, dto);
                return Ok(new
                {
                    success = true,
                    message = "Report sent successfully",
                    data = report
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reporting post {PostId}", postId);
                return StatusCode(500, new { success = false, message = "An error occurred" });
            }
        }
        /// <summary>Get all reports - Admin only</summary>
        [HttpGet("reports")]
        [Authorize(Roles = "ADMIN")]
        public async Task<ActionResult> GetAllReports()
        {
            try
            {
                var reports = await _communityService.GetAllReportsAsync();
                return Ok(new
                {
                    success = true,
                    message = "Reports retrieved successfully",
                    count = reports.Count,
                    data = reports
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all reports");
                return StatusCode(500, new { success = false, message = "An error occurred" });
            }
        }

        /// <summary>Get report by ID - Admin only</summary>
        [HttpGet("reports/{id}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<ActionResult> GetReportById(int id)
        {
            try
            {
                var report = await _communityService.GetReportByIdAsync(id);
                return Ok(new { success = true, message = "Report retrieved successfully", data = report });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving report {Id}", id);
                return StatusCode(500, new { success = false, message = "An error occurred" });
            }
        }

        /// <summary>Get pending reports - Admin only</summary>
        [HttpGet("reports/pending")]
        [Authorize(Roles = "ADMIN")]
        public async Task<ActionResult> GetPendingReports()
        {
            try
            {
                var reports = await _communityService.GetPendingReportsAsync();
                return Ok(new
                {
                    success = true,
                    message = "Pending reports retrieved successfully",
                    count = reports.Count,
                    data = reports
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving pending reports");
                return StatusCode(500, new { success = false, message = "An error occurred" });
            }
        }

        /// <summary>Get reviewed reports - Admin only</summary>
        [HttpGet("reports/reviewed")]
        [Authorize(Roles = "ADMIN")]
        public async Task<ActionResult> GetReviewedReports()
        {
            try
            {
                var reports = await _communityService.GetReviewedReportsAsync();
                return Ok(new
                {
                    success = true,
                    message = "Reviewed reports retrieved successfully",
                    count = reports.Count,
                    data = reports
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving reviewed reports");
                return StatusCode(500, new { success = false, message = "An error occurred" });
            }
        }

        [HttpPut("reports/{id}/review")]
        [Authorize(Roles = "ADMIN")]
        public async Task<ActionResult> ReviewReport(int id, [FromBody] ReviewReportDto dto)
        {
            try
            {
                var adminId = GetCurrentUserId();
                var report = await _communityService.ReviewReportAsync(id, adminId, dto);
                return Ok(new { success = true, message = "Report reviewed successfully", data = report });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reviewing report {Id}", id);
                return StatusCode(500, new { success = false, message = "An error occurred" });
            }
        }


    }
}