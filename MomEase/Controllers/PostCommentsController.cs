using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MomEase.core.DTOS.CommunityDTO;
using MomEase.core.Interfaces;
using System.Security.Claims;

namespace MomEase.api.Controllers
{
    [ApiController]
    [Route("api/community/posts/{postId}/comments")]
    [Authorize]
    public class PostCommentsController : ControllerBase
    {
        private readonly ICommunityService _communityService;
        private readonly ILogger<PostCommentsController> _logger;

        public PostCommentsController(
            ICommunityService communityService,
            ILogger<PostCommentsController> logger)
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

        /// <summary>Add comment on post</summary>
        [HttpPost]
        public async Task<ActionResult> AddComment(int postId, [FromBody] CreateCommentDto dto)
        {
            try
            {
                var userId = GetCurrentUserId();
                var comment = await _communityService.AddCommentAsync(postId, userId, dto);
                return CreatedAtAction(
                    nameof(GetCommentById),
                    new { postId, id = comment.CommentId },
                    new { success = true, message = "Comment added successfully", data = comment });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding comment on post {PostId}", postId);
                return StatusCode(500, new { success = false, message = "An error occurred" });
            }
        }

        /// <summary>Get all comments on post</summary>
        [HttpGet]
        public async Task<ActionResult> GetPostComments(int postId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var comments = await _communityService.GetPostCommentsAsync(postId, userId);
                return Ok(new
                {
                    success = true,
                    message = "Comments retrieved successfully",
                    count = comments.Count,
                    data = comments
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving comments for post {PostId}", postId);
                return StatusCode(500, new { success = false, message = "An error occurred" });
            }
        }

        /// <summary>Get comment by ID</summary>
        [HttpGet("{id}")]
        public async Task<ActionResult> GetCommentById(int postId, int id)
        {
            try
            {
                var comment = await _communityService.GetCommentByIdAsync(id, postId);
                return Ok(new { success = true, message = "Comment retrieved successfully", data = comment });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving comment {Id}", id);
                return StatusCode(500, new { success = false, message = "An error occurred" });
            }
        }

        /// <summary>Update comment</summary>
        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateComment(
            int postId, int id, [FromBody] UpdateCommentDto dto)
        {
            try
            {
                var userId = GetCurrentUserId();
                var comment = await _communityService.UpdateCommentAsync(id, postId, userId, dto);
                return Ok(new { success = true, message = "Comment updated successfully", data = comment });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { success = false, message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating comment {Id}", id);
                return StatusCode(500, new { success = false, message = "An error occurred" });
            }
        }

        /// <summary>Delete comment</summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteComment(int postId, int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                var isAdmin = User.IsInRole("ADMIN");
                await _communityService.DeleteCommentAsync(id, postId, userId, isAdmin);
                return Ok(new { success = true, message = "Comment deleted successfully" });
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
                _logger.LogError(ex, "Error deleting comment {Id}", id);
                return StatusCode(500, new { success = false, message = "An error occurred" });
            }
        }
    }
}