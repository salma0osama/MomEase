using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PostCare.core.DTOS.SleepRecordDTO;
using PostCare.core.Interfaces;
using System.Security.Claims;

namespace PostCare.api.Controllers
{
    [ApiController]
    [Route("api/children/{childId}/sleep-records")]
    [Authorize(Roles = "MOTHER")]
    public class SleepRecordsController : ControllerBase
    {
        private readonly ISleepRecordService _sleepRecordService;
        private readonly IChildRepository _childRepository;
        private readonly ILogger<SleepRecordsController> _logger;

        public SleepRecordsController(
            ISleepRecordService sleepRecordService,
            IChildRepository childRepository,
            ILogger<SleepRecordsController> logger)
        {
            _sleepRecordService = sleepRecordService ?? throw new ArgumentNullException(nameof(sleepRecordService));
            _childRepository = childRepository ?? throw new ArgumentNullException(nameof(childRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// ✅ إضافة: Helper method للتحقق من ملكية الطفل (مثل Feeding)
        /// </summary>
        private async Task<(int userId, bool isOwner)> VerifyChildOwnershipAsync(int childId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                throw new UnauthorizedAccessException("User ID not found in token");
            }

            var isOwner = await _childRepository.IsChildOwnedByUserAsync(childId, userId);

            return (userId, isOwner);
        }

        /// <summary>
        /// إضافة سجل نوم جديد
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<SleepRecordDto>> CreateSleepRecord(
             int childId,
            [FromBody] CreateSleepRecordDto dto)
        {
            try
            {
                // ✅ 1. التحقق من وجود الطفل أولاً
                var child = await _childRepository.GetChildByIdAsync(childId);
                if (child == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = $"Child with ID {childId} not found"
                    });
                }

                // ✅ 2. التحقق من الملكية
                var (userId, isOwner) = await VerifyChildOwnershipAsync(childId);
                if (!isOwner)
                {
                    _logger.LogWarning("User {UserId} attempted to create sleep record for child {ChildId} they don't own", userId, childId);
                    return StatusCode(403, new
                    {
                        success = false,
                        message = "You don't have permission to add sleep records for this child"
                    });
                }

                // ✅ 3. إنشاء السجل
                dto.ChildId = childId;
                var record = await _sleepRecordService.CreateSleepRecordAsync(userId, dto);

                return CreatedAtAction(
                    nameof(GetSleepRecordById),
                    new { childId = childId, id = record.RecordId },
                    new { success = true, message = "Sleep record added successfully", data = record }
                );
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation when creating sleep record");
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid argument when creating sleep record");
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating sleep record: {Error}", ex.Message);
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while creating the sleep record"
                });
            }
        }

        /// <summary>
        /// الحصول على جميع سجلات النوم للطفل
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<List<SleepRecordDto>>> GetChildSleepRecords([FromRoute] int childId)
        {
            try
            {
                // ✅ 1. التحقق من وجود الطفل
                var child = await _childRepository.GetChildByIdAsync(childId);
                if (child == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = $"Child with ID {childId} not found"
                    });
                }

                // ✅ 2. التحقق من الملكية
                var (userId, isOwner) = await VerifyChildOwnershipAsync(childId);
                if (!isOwner)
                {
                    return StatusCode(403, new
                    {
                        success = false,
                        message = "You don't have permission to view this child's sleep records"
                    });
                }

                var records = await _sleepRecordService.GetChildSleepRecordsAsync(childId, userId);

                return Ok(new
                {
                    success = true,
                    message = "Data retrieved successfully",
                    count = records.Count,
                    data = records
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving sleep records for child {ChildId}", childId);
                return StatusCode(500, new { success = false, message = "An error occurred while retrieving data" });
            }
        }

        /// <summary>
        /// الحصول على سجل نوم معين
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<SleepRecordDto>> GetSleepRecordById(
            [FromRoute] int childId,
            [FromRoute] int id)
        {
            try
            {
                // ✅ 1. التحقق من وجود الطفل
                var child = await _childRepository.GetChildByIdAsync(childId);
                if (child == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = $"Child with ID {childId} not found"
                    });
                }

                // ✅ 2. التحقق من الملكية
                var (userId, isOwner) = await VerifyChildOwnershipAsync(childId);
                if (!isOwner)
                {
                    return StatusCode(403, new
                    {
                        success = false,
                        message = "You don't have permission to view this sleep record"
                    });
                }

                var record = await _sleepRecordService.GetSleepRecordByIdAsync(id, userId);

                // ✅ 3. التحقق من أن السجل يخص الطفل المحدد
                if (record.ChildId != childId)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "This sleep record does not belong to the specified child"
                    });
                }

                return Ok(new { success = true, message = "Data retrieved successfully", data = record });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving sleep record {RecordId}", id);
                return StatusCode(500, new { success = false, message = "An error occurred while retrieving data" });
            }
        }

        /// <summary>
        /// تحديث سجل نوم
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<SleepRecordDto>> UpdateSleepRecord(
            [FromRoute] int childId,
            [FromRoute] int id,
            [FromBody] UpdateSleepRecordDto dto)
        {
            try
            {
                // ✅ 1. التحقق من وجود الطفل
                var child = await _childRepository.GetChildByIdAsync(childId);
                if (child == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = $"Child with ID {childId} not found"
                    });
                }

                // ✅ 2. التحقق من الملكية
                var (userId, isOwner) = await VerifyChildOwnershipAsync(childId);
                if (!isOwner)
                {
                    return StatusCode(403, new
                    {
                        success = false,
                        message = "You don't have permission to update this sleep record"
                    });
                }

                var record = await _sleepRecordService.UpdateSleepRecordAsync(id, userId, dto);

                // ✅ 3. التحقق من أن السجل يخص الطفل المحدد
                if (record.ChildId != childId)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "This sleep record does not belong to the specified child"
                    });
                }

                return Ok(new { success = true, message = "Sleep record updated successfully", data = record });
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
                _logger.LogError(ex, "Error updating sleep record {RecordId}", id);
                return StatusCode(500, new { success = false, message = "An error occurred while updating data" });
            }
        }

        /// <summary>
        /// حذف سجل نوم
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> DeleteSleepRecord(
            [FromRoute] int childId,
            [FromRoute] int id)
        {
            try
            {
                // ✅ 1. التحقق من وجود الطفل
                var child = await _childRepository.GetChildByIdAsync(childId);
                if (child == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = $"Child with ID {childId} not found"
                    });
                }

                // ✅ 2. التحقق من الملكية
                var (userId, isOwner) = await VerifyChildOwnershipAsync(childId);
                if (!isOwner)
                {
                    return StatusCode(403, new
                    {
                        success = false,
                        message = "You don't have permission to delete this sleep record"
                    });
                }

                // ✅ 3. التحقق من أن السجل يخص الطفل المحدد قبل الحذف
                var record = await _sleepRecordService.GetSleepRecordByIdAsync(id, userId);
                if (record.ChildId != childId)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "This sleep record does not belong to the specified child"
                    });
                }

                await _sleepRecordService.DeleteSleepRecordAsync(id, userId);

                return Ok(new { success = true, message = "Sleep record deleted successfully" });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting sleep record {RecordId}", id);
                return StatusCode(500, new { success = false, message = "An error occurred while deleting the record" });
            }
        }

        /// <summary>
        /// إحصائيات النوم
        /// </summary>
        [HttpGet("statistics")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<SleepStatisticsDto>> GetSleepStatistics([FromRoute] int childId)
        {
            try
            {
                var (userId, isOwner) = await VerifyChildOwnershipAsync(childId);
                if (!isOwner)
                {
                    return StatusCode(403, new
                    {
                        success = false,
                        message = "You don't have permission to view this child's statistics"
                    });
                }

                var statistics = await _sleepRecordService.GetSleepStatisticsAsync(childId, userId);

                return Ok(new { success = true, message = "Statistics retrieved successfully", data = statistics });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving sleep statistics for child {ChildId}", childId);
                return StatusCode(500, new { success = false, message = "An error occurred while retrieving statistics" });
            }
        }

        /// <summary>
        /// النوم الأسبوعي
        /// </summary>
        [HttpGet("weekly")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<WeeklySleepDto>> GetWeeklySleep([FromRoute] int childId)
        {
            try
            {
                var (userId, isOwner) = await VerifyChildOwnershipAsync(childId);
                if (!isOwner)
                {
                    return StatusCode(403, new
                    {
                        success = false,
                        message = "You don't have permission to view this child's weekly records"
                    });
                }

                var weeklySleep = await _sleepRecordService.GetWeeklySleepAsync(childId, userId);

                return Ok(new { success = true, message = "Weekly sleep data retrieved successfully", data = weeklySleep });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving weekly sleep data for child {ChildId}", childId);
                return StatusCode(500, new { success = false, message = "An error occurred while retrieving weekly data" });
            }
        }

        /// <summary>
        /// النوم الشهري
        /// </summary>
        [HttpGet("monthly")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<MonthlySleepDto>> GetMonthlySleep([FromRoute] int childId)
        {
            try
            {
                var (userId, isOwner) = await VerifyChildOwnershipAsync(childId);
                if (!isOwner)
                {
                    return StatusCode(403, new
                    {
                        success = false,
                        message = "You don't have permission to view this child's monthly records"
                    });
                }

                var monthlySleep = await _sleepRecordService.GetMonthlySleepAsync(childId, userId);

                return Ok(new { success = true, message = "Monthly sleep data retrieved successfully", data = monthlySleep });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving monthly sleep data for child {ChildId}", childId);
                return StatusCode(500, new { success = false, message = "An error occurred while retrieving monthly data" });
            }
        }
    }
}