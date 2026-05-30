using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MomEase.core.Interfaces;
using System.Security.Claims;

namespace MomEase.api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "MOTHER")]
    public class TrackingRemindersController : ControllerBase
    {
        private readonly IDailyTrackingReminderService _service;
        private readonly ILogger<TrackingRemindersController> _logger;

        public TrackingRemindersController(
            IDailyTrackingReminderService service,
            ILogger<TrackingRemindersController> logger)
        {
            _service = service;
            _logger = logger;
        }

        /// <summary>
        /// Get today's tracking status for a child
        /// </summary>
        [HttpGet("status")]
        public async Task<IActionResult> GetTrackingStatus()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out int userId))
                    return Unauthorized();

                var status = await _service.GetTrackingStatusAsync(userId);

                if (status == null)
                    return NotFound(new { error = "No children found" });

                return Ok(new { success = true, data = status });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting tracking status");
                return StatusCode(500, new { error = "An error occurred" });
            }
        }
        [HttpPost("send-now")]
        public async Task<IActionResult> SendNow()
        {
            var logs = new List<string>();
            try
            {
                await _service.SendDailyTrackingRemindersAsync();
                return Ok(new { success = true, message = "Reminders sent" });
            }
            catch (Exception ex)
            {
                // ← هيرجعلك الـ error مباشرة في Postman/Swagger
                return StatusCode(500, new
                {
                    error = ex.Message,
                    innerError = ex.InnerException?.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }
    }
}
