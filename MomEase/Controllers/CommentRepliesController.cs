using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MomEase.core.DTOS.CommunityDTO;
using MomEase.core.Interfaces;
using System.Security.Claims;

namespace MomEase.api.Controllers
{
    [ApiController]
    [Route("api/community/posts/{postId}/comments/{commentId}/replies")]
    [Authorize]
    public class CommentRepliesController : ControllerBase
    {
        private readonly ICommunityService _communityService;
        private readonly ILogger<CommentRepliesController> _logger;

        public CommentRepliesController(
            ICommunityService communityService,
            ILogger<CommentRepliesController> logger)
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
        public async Task<ActionResult> AddReply(
            int commentId, [FromBody] CreateReplyDto dto)
        {
            try
            {
                var userId = GetCurrentUserId();
                var reply = await _communityService.AddReplyAsync(commentId, userId, dto);
                return Ok(new
                {
                    success = true,
                    message = "Reply added successfully",
                    data = reply
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
                _logger.LogError(ex, "Error adding reply to comment {CommentId}", commentId);
                return StatusCode(500, new { success = false, message = "An error occurred" });
            }
        }

        [HttpGet]
        public async Task<ActionResult> GetCommentReplies(int commentId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var replies = await _communityService
                    .GetCommentRepliesAsync(commentId, userId);
                return Ok(new
                {
                    success = true,
                    message = "Replies retrieved successfully",
                    count = replies.Count,
                    data = replies
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving replies for comment {CommentId}", commentId);
                return StatusCode(500, new { success = false, message = "An error occurred" });
            }
        }

        [HttpPut("{replyId}")]
        public async Task<ActionResult> UpdateReply(
            int commentId, int replyId, [FromBody] UpdateReplyDto dto)
        {
            try
            {
                var userId = GetCurrentUserId();
                var reply = await _communityService
                    .UpdateReplyAsync(replyId, commentId, userId, dto);
                return Ok(new
                {
                    success = true,
                    message = "Reply updated successfully",
                    data = reply
                });
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
                _logger.LogError(ex, "Error updating reply {ReplyId}", replyId);
                return StatusCode(500, new { success = false, message = "An error occurred" });
            }
        }

        [HttpDelete("{replyId}")]
        public async Task<ActionResult> DeleteReply(int commentId, int replyId)
        {
            try
            {
                var userId = GetCurrentUserId();
                await _communityService.DeleteReplyAsync(replyId, commentId, userId);
                return Ok(new { success = true, message = "Reply deleted successfully" });
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
                _logger.LogError(ex, "Error deleting reply {ReplyId}", replyId);
                return StatusCode(500, new { success = false, message = "An error occurred" });
            }
        }
    }
}