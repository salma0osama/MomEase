using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PostCare.core.DTOS.ChildDTO;
using PostCare.core.Interfaces;
using System.Security.Claims;

namespace PostCare.api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ChildrenController : ControllerBase
    {
        private readonly IChildService _childService;
        private readonly ILogger<ChildrenController> _logger;

        public ChildrenController(
            IChildService childService,
            ILogger<ChildrenController> logger)
        {
            _childService = childService;
            _logger = logger;
        }

        // Helper method to get current user ID from JWT token
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
        /// Add a new child
        /// </summary>
        /// <param name="dto">Child data</param>
        /// <returns>Created child data</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ChildDto>> CreateChild([FromBody] CreateChildDto dto)
        {
            try
            {
                var userId = GetCurrentUserId();
                var child = await _childService.CreateChildAsync(userId, dto);

                return CreatedAtAction(
                    nameof(GetChildById),
                    new { ChildId = child.ChildId },
                    new { success = true, message = "Child added successfully", data = child }
                );
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid data provided when creating a child");
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access attempt");
                return Unauthorized(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {

                _logger.LogError(ex, "Error creating child: {Error}", ex.ToString());

                return StatusCode(500, new
                {
                    success = false,
                    message = ex.Message,
                    type = ex.GetType().Name,
                    innerException = ex.InnerException?.Message,
                    stackTrace = ex.StackTrace?.Split('\n').Take(5)
                });
            }
        }

        /// <summary>
        /// Get all children of the current user
        /// </summary>
        /// <returns>List of user's children</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<List<ChildDto>>> GetUserChildren()
        {
            try
            {
                var userId = GetCurrentUserId();
                var children = await _childService.GetUserChildrenAsync(userId);

                return Ok(new
                {
                    success = true,
                    message = "Data retrieved successfully",
                    count = children.Count,
                    data = children
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access attempt");
                return Unauthorized(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user's children");
                return StatusCode(500, new { success = false, message = "An error occurred while retrieving data" });
            }
        }

        /// <summary>
        /// Get a specific child by ID
        /// </summary>
        /// <param name="id">Child ID</param>
        /// <returns>Child data</returns>
        [HttpGet("{ChildId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ChildDto>> GetChildById(int ChildId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var child = await _childService.GetChildByIdAsync(ChildId, userId);

                return Ok(new { success = true, message = "Data retrieved successfully", data = child });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Child not found: {ChildId}", ChildId);
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access attempt for child: {ChildId}", ChildId);
                return StatusCode(403, new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving child data: {ChildId}", ChildId);
                return StatusCode(500, new { success = false, message = "An error occurred while retrieving data" });
            }
        }

        /// <summary>
        /// Update child data
        /// </summary>
        /// <param name="id">Child ID</param>
        /// <param name="dto">Updated data</param>
        /// <returns>Updated child data</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ChildDto>> UpdateChild(int id, [FromBody] UpdateChildDto dto)
        {
            try
            {
                var userId = GetCurrentUserId();
                var child = await _childService.UpdateChildAsync(id, userId, dto);

                return Ok(new { success = true, message = "Child data updated successfully", data = child });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Child not found: {ChildId}", id);
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized update attempt for child: {ChildId}", id);
                return StatusCode(403, new { success = false, message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid data provided when updating child: {ChildId}", id);
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating child: {ChildId}", id);
                return StatusCode(500, new { success = false, message = "An error occurred while updating data" });
            }
        }

        /// <summary>
        /// Delete a child
        /// </summary>
        /// <param name="id">Child ID</param>
        /// <returns>Deletion result</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult> DeleteChild(int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                await _childService.DeleteChildAsync(id, userId);

                return Ok(new { success = true, message = "Child deleted successfully" });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Child not found: {ChildId}", id);
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized deletion attempt for child: {ChildId}", id);
                return StatusCode(403, new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting child: {ChildId}", id);
                return StatusCode(500, new { success = false, message = "An error occurred while deleting the child" });
            }
        }

        /// <summary>
        /// Upload a child photo
        /// </summary>
        /// <param name="id">Child ID</param>
        /// <param name="photo">Image file (JPG, JPEG, PNG - Max 5MB)</param>
        /// <returns>Photo URL</returns>
        [HttpPost("{id}/photo")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult> UploadChildPhoto(
            [FromRoute] int id,
             IFormFile photo)
        {
            try
            {
                var userId = GetCurrentUserId();
                var photoUrl = await _childService.UploadChildPhotoAsync(id, userId, photo);

                return Ok(new
                {
                    success = true,
                    message = "Photo uploaded successfully",
                    data = new { photoUrl }
                });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Child not found: {ChildId}", id);
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized photo upload attempt for child: {ChildId}", id);
                return StatusCode(403, new { success = false, message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid file uploaded for child: {ChildId}", id);
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading child photo: {ChildId} - {Error}", id, ex.ToString());

                return StatusCode(500, new
                {
                    success = false,
                    message = ex.Message,
                    innerException = ex.InnerException?.Message,
                    stackTrace = ex.StackTrace?.Split('\n').Take(3).ToList()
                });
            }
        }

        /// <summary>
        /// Delete a child photo
        /// </summary>
        /// <param name="id">Child ID</param>
        /// <returns>Deletion result</returns>
        [HttpDelete("{id}/photo")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult> DeleteChildPhoto([FromRoute] int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                await _childService.DeleteChildPhotoAsync(id, userId);

                return Ok(new { success = true, message = "Photo deleted successfully" });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Child not found: {ChildId}", id);
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized photo deletion attempt for child: {ChildId}", id);
                return StatusCode(403, new { success = false, message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "No photo to delete for child: {ChildId}", id);
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting child photo: {ChildId}", id);
                return StatusCode(500, new { success = false, message = "An error occurred while deleting the photo" });
            }
        }
    }
}
