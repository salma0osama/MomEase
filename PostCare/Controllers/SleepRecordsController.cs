using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PostCare.core.DTOS.SleepRecordDTO;
using PostCare.core.Interfaces;
using System.Security.Claims;

namespace PostCare.api.Controllers
{
    [ApiController]
    [Route("api/children/{childId}/sleep-records")]
    [Authorize]
    public class SleepRecordsController : ControllerBase
    {
        private readonly ISleepRecordService _sleepRecordService;
        private readonly ILogger<SleepRecordsController> _logger;

        public SleepRecordsController(
            ISleepRecordService sleepRecordService,
            ILogger<SleepRecordsController> logger)
        {
            _sleepRecordService = sleepRecordService;
            _logger = logger;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                           ?? User.FindFirst("userId")?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                throw new UnauthorizedAccessException("User ID not found");
            }

            return userId;
        }

        /// <summary>
        /// إضافة سجل نوم جديد
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<SleepRecordDto>> CreateSleepRecord(
            [FromRoute] int childId,
            [FromBody] CreateSleepRecordDto dto)
        {
            try
            {
                dto.ChildId = childId;
                var userId = GetCurrentUserId();
                var record = await _sleepRecordService.CreateSleepRecordAsync(userId, dto);

                return CreatedAtAction(
                    nameof(GetSleepRecordById),
                    new { childId = childId, id = record.RecordId },
                    new { success = true, message = "Sleep record added successfully", data = record }
                );
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating sleep record");
                return StatusCode(500, new { success = false, message = "An error occurred while adding the sleep record" });
            }
        }

        /// <summary>
        /// الحصول على جميع سجلات النوم للطفل
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<List<SleepRecordDto>>> GetChildSleepRecords([FromRoute] int childId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var records = await _sleepRecordService.GetChildSleepRecordsAsync(childId, userId);

                return Ok(new
                {
                    success = true,
                    message = "Data retrieved successfully",
                    count = records.Count,
                    data = records
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving sleep records");
                return StatusCode(500, new { success = false, message = "An error occurred while retrieving data" });
            }
        }

        /// <summary>
        /// الحصول على سجل نوم معين
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<SleepRecordDto>> GetSleepRecordById(
            [FromRoute] int childId,
            [FromRoute] int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                var record = await _sleepRecordService.GetSleepRecordByIdAsync(id, userId);

                return Ok(new { success = true, message = "Data retrieved successfully", data = record });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving sleep record");
                return StatusCode(500, new { success = false, message = "An error occurred while retrieving data" });
            }
        }

        /// <summary>
        /// تحديث سجل نوم
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<SleepRecordDto>> UpdateSleepRecord(
            [FromRoute] int childId,
            [FromRoute] int id,
            [FromBody] UpdateSleepRecordDto dto)
        {
            try
            {
                var userId = GetCurrentUserId();
                var record = await _sleepRecordService.UpdateSleepRecordAsync(id, userId, dto);

                return Ok(new { success = true, message = "Sleep record updated successfully", data = record });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating sleep record");
                return StatusCode(500, new { success = false, message = "An error occurred while updating data" });
            }
        }

        /// <summary>
        /// حذف سجل نوم
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> DeleteSleepRecord(
            [FromRoute] int childId,
            [FromRoute] int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                await _sleepRecordService.DeleteSleepRecordAsync(id, userId);

                return Ok(new { success = true, message = "Sleep record deleted successfully" });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting sleep record");
                return StatusCode(500, new { success = false, message = "An error occurred while deleting the record" });
            }
        }
        /// <summary>
        /// إحصائيات النوم
        /// </summary>
        [HttpGet("statistics")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<SleepStatisticsDto>> GetSleepStatistics([FromRoute] int childId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var statistics = await _sleepRecordService.GetSleepStatisticsAsync(childId, userId);

                return Ok(new { success = true, message = "Statistics retrieved successfully", data = statistics });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving sleep statistics");
                return StatusCode(500, new { success = false, message = "An error occurred while retrieving statistics" });
            }
        }

        /// <summary>
        /// النوم الأسبوعي
        /// </summary>
        [HttpGet("weekly")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<WeeklySleepDto>> GetWeeklySleep([FromRoute] int childId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var weeklySleep = await _sleepRecordService.GetWeeklySleepAsync(childId, userId);

                return Ok(new { success = true, message = "Weekly sleep data retrieved successfully", data = weeklySleep });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving weekly sleep data");
                return StatusCode(500, new { success = false, message = "An error occurred while retrieving weekly data" });
            }
        }

        /// <summary>
        /// النوم الشهري
        /// </summary>
        [HttpGet("monthly")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<MonthlySleepDto>> GetMonthlySleep([FromRoute] int childId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var monthlySleep = await _sleepRecordService.GetMonthlySleepAsync(childId, userId);

                return Ok(new { success = true, message = "Monthly sleep data retrieved successfully", data = monthlySleep });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving monthly sleep data");
                return StatusCode(500, new { success = false, message = "An error occurred while retrieving monthly data" });
            }
        }
    }
}
