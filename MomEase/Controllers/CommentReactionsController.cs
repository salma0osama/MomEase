using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MomEase.core.DTOS.CommunityDTO;
using MomEase.core.Interfaces;
using System.Security.Claims;

namespace MomEase.api.Controllers
{
    [ApiController]
    [Route("api/community/posts/{postId}/comments/{commentId}/reactions")]
    [Authorize]
    public class CommentReactionsController : ControllerBase
    {
        private readonly ICommunityService _communityService;
        private readonly ILogger<CommentReactionsController> _logger;

        public CommentReactionsController(
            ICommunityService communityService,
            ILogger<CommentReactionsController> logger)
        {
            _communityService = communityService;
            _logger = logger;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                           ?? User.FindFirst("userId")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) ||
                !int.TryParse(userIdClaim, out int userId))
                throw new UnauthorizedAccessException("User ID not found");
            return userId;
        }

        [HttpPost]
        public async Task<ActionResult> AddReaction(
            int commentId, [FromBody] AddCommentReactionDto dto)
        {
            try
            {
                var userId = GetCurrentUserId();
                var reaction = await _communityService
                    .AddCommentReactionAsync(commentId, userId, dto);
                return Ok(new
                {
                    success = true,
                    message = "Reaction added successfully",
                    data = reaction
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
                _logger.LogError(ex, "Error adding reaction to comment {CommentId}", commentId);
                return StatusCode(500, new { success = false, message = "An error occurred" });
            }
        }

        [HttpGet]
        public async Task<ActionResult> GetCommentReactions(int commentId)
        {
            try
            {
                var reactions = await _communityService.GetCommentReactionsAsync(commentId);
                return Ok(new
                {
                    success = true,
                    message = "Reactions retrieved successfully",
                    count = reactions.Count,
                    data = reactions
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving reactions for comment {CommentId}", commentId);
                return StatusCode(500, new { success = false, message = "An error occurred" });
            }
        }

        [HttpGet("count")]
        public async Task<ActionResult> GetReactionsCount(int commentId)
        {
            try
            {
                var count = await _communityService
                    .GetCommentReactionsCountAsync(commentId);
                return Ok(new
                {
                    success = true,
                    message = "Reactions count retrieved successfully",
                    data = count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving reactions count for comment {CommentId}", commentId);
                return StatusCode(500, new { success = false, message = "An error occurred" });
            }
        }

        [HttpPut]
        public async Task<ActionResult> UpdateReaction(
            int commentId, [FromBody] UpdateCommentReactionDto dto)
        {
            try
            {
                var userId = GetCurrentUserId();
                var reaction = await _communityService
                    .UpdateCommentReactionAsync(commentId, userId, dto);
                return Ok(new
                {
                    success = true,
                    message = "Reaction updated successfully",
                    data = reaction
                });
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
                _logger.LogError(ex, "Error updating reaction for comment {CommentId}", commentId);
                return StatusCode(500, new { success = false, message = "An error occurred" });
            }
        }

        [HttpDelete]
        public async Task<ActionResult> DeleteReaction(int commentId)
        {
            try
            {
                var userId = GetCurrentUserId();
                await _communityService.DeleteCommentReactionAsync(commentId, userId);
                return Ok(new { success = true, message = "Reaction deleted successfully" });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting reaction for comment {CommentId}", commentId);
                return StatusCode(500, new { success = false, message = "An error occurred" });
            }
        }
    }
}