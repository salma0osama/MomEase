
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MomEase.core.DTOS.FeedingRecordDto;
using MomEase.core.Interfaces;
using System.Security.Claims;

namespace MomEase.api.Controllers
{
    [Route("api/children/{childId}/feeding-records")]
    [ApiController]
    [Authorize(Roles = "MOTHER")]
    public class FeedingRecordsController : ControllerBase
    {
        private readonly IFeedingRecordService _feedingRecordService;
        private readonly IChildRepository _childRepository;
        private readonly ILogger<FeedingRecordsController> _logger;

        public FeedingRecordsController(
            IFeedingRecordService feedingRecordService,
            IChildRepository childRepository,
            ILogger<FeedingRecordsController> logger)
        {
            _feedingRecordService = feedingRecordService ?? throw new ArgumentNullException(nameof(feedingRecordService));
            _childRepository = childRepository ?? throw new ArgumentNullException(nameof(childRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Helper method to get current user ID and verify child ownership
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
        /// POST /api/children/{childId}/feeding-records - Add new feeding record
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CreateFeedingRecord(int childId, [FromBody] CreateFeedingRecordDto createDto)
        {
            try
            {
                //  التحقق من وجود الطفل أولاً
                var child = await _childRepository.GetChildByIdAsync(childId);

                if (child == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = $"Child with ID {childId} not found"
                    });
                }
                // Verify ownership BEFORE creating
                var (userId, isOwner) = await VerifyChildOwnershipAsync(childId);

                if (!isOwner)
                {
                    _logger.LogWarning("User {UserId} attempted to create feeding record for child {ChildId} they don't own", userId, childId);
                    return StatusCode(403, new
                    {
                        success = false,
                        message = "You don't have permission to add feeding records for this child"
                    });
                }

                var record = await _feedingRecordService.CreateFeedingRecordAsync(childId, createDto);

                return CreatedAtAction(
                    nameof(GetRecordById),
                    new { childId, id = record.RecordId },
                    new
                    {
                        success = true,
                        message = "Feeding record created successfully",
                        data = record
                    });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Child not found: {ChildId}", childId);
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation when creating feeding record");
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid argument when creating feeding record");
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access attempt");
                return Unauthorized(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating feeding record for child {ChildId}", childId);
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while creating the feeding record"
                });
            }
        }

        /// <summary>
        /// GET /api/children/{childId}/feeding-records - Get all feeding records for child
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetAllRecords(int childId)
        {
            try
            {
                // 1. التحقق من وجود الطفل أولاً
                var child = await _childRepository.GetChildByIdAsync(childId);

                if (child == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = $"Child with ID {childId} not found"
                    });
                }
                var (userId, isOwner) = await VerifyChildOwnershipAsync(childId);

                if (!isOwner)
                {
                    return StatusCode(403, new
                    {
                        success = false,
                        message = "You don't have permission to view this child's feeding records"
                    });
                }

                var records = await _feedingRecordService.GetAllRecordsForChildAsync(childId);

                return Ok(new
                {
                    success = true,
                    message = "Feeding records retrieved successfully",
                    count = records.Count,
                    data = records
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving feeding records for child {ChildId}", childId);
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while retrieving feeding records"
                });
            }
        }

        /// <summary>
        /// GET /api/children/{childId}/feeding-records/{id} - Get specific feeding record
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetRecordById(int childId, int id)
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
                var (userId, isOwner) = await VerifyChildOwnershipAsync(childId);

                if (!isOwner)
                {
                    return StatusCode(403, new
                    {
                        success = false,
                        message = "You don't have permission to view this feeding record"
                    });
                }

                var record = await _feedingRecordService.GetRecordByIdAsync(id);

                // Verify record belongs to the specified child
                if (record.ChildId != childId)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "This feeding record does not belong to the specified child"
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = "Feeding record retrieved successfully",
                    data = record
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving feeding record {RecordId}", id);
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while retrieving the feeding record"
                });
            }
        }

        /// <summary>
        /// PUT /api/children/{childId}/feeding-records/{id} - Update feeding record
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateRecord(int childId, int id, [FromBody] UpdateFeedingRecordDto updateDto)
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

                var (userId, isOwner) = await VerifyChildOwnershipAsync(childId);

                if (!isOwner)
                {
                    return StatusCode(403, new
                    {
                        success = false,
                        message = "You don't have permission to update this feeding record"
                    });
                }

                var record = await _feedingRecordService.UpdateFeedingRecordAsync(id, updateDto);

                // Verify record belongs to the specified child
                if (record.ChildId != childId)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "This feeding record does not belong to the specified child"
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = "Feeding record updated successfully",
                    data = record
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
                _logger.LogError(ex, "Error updating feeding record {RecordId}", id);
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while updating the feeding record"
                });
            }
        }

        /// <summary>
        /// DELETE /api/children/{childId}/feeding-records/{id} - Delete feeding record
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteRecord(int childId, int id)
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

                var (userId, isOwner) = await VerifyChildOwnershipAsync(childId);

                if (!isOwner)
                {
                    return StatusCode(403, new
                    {
                        success = false,
                        message = "You don't have permission to delete this feeding record"
                    });
                }

                // Verify record exists and belongs to child before deleting
                var record = await _feedingRecordService.GetRecordByIdAsync(id);

                if (record.ChildId != childId)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "This feeding record does not belong to the specified child"
                    });
                }

                await _feedingRecordService.DeleteFeedingRecordAsync(id);

                return Ok(new
                {
                    success = true,
                    message = "Feeding record deleted successfully"
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting feeding record {RecordId}", id);
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while deleting the feeding record"
                });
            }
        }

        /// <summary>
        /// GET /api/children/{childId}/feeding-records/statistics - Get feeding statistics
        /// </summary>
        [HttpGet("statistics")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetStatistics(int childId)
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

                var statistics = await _feedingRecordService.GetFeedingStatisticsAsync(childId);

                return Ok(new
                {
                    success = true,
                    message = "Feeding statistics retrieved successfully",
                    data = statistics
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving feeding statistics for child {ChildId}", childId);
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while retrieving statistics"
                });
            }
        }

        /// <summary>
        /// GET /api/children/{childId}/feeding-records/weekly - Get weekly feeding records
        /// </summary>
        [HttpGet("weekly")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetWeeklyRecords(int childId)
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

                var weeklyData = await _feedingRecordService.GetWeeklyRecordsAsync(childId);

                return Ok(new
                {
                    success = true,
                    message = "Weekly feeding records retrieved successfully",
                    data = weeklyData
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving weekly feeding records for child {ChildId}", childId);
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while retrieving weekly records"
                });
            }
        }

        /// <summary>
        /// GET /api/children/{childId}/feeding-records/monthly - Get monthly feeding records
        /// </summary>
        [HttpGet("monthly")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetMonthlyRecords(int childId)
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

                var monthlyData = await _feedingRecordService.GetMonthlyRecordsAsync(childId);

                return Ok(new
                {
                    success = true,
                    message = "Monthly feeding records retrieved successfully",
                    data = monthlyData
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving monthly feeding records for child {ChildId}", childId);
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while retrieving monthly records"
                });
            }
        }
    }
}
