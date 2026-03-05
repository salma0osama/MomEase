using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MomEase.core.DTOS.CommunityDTO;
using MomEase.core.Interfaces;
using System.Security.Claims;

namespace MomEase.api.Controllers
{
    [ApiController]
    [Route("api/community/posts/{postId}/reactions")]
    [Authorize]
    public class PostReactionsController : ControllerBase
    {
        private readonly ICommunityService _communityService;
        private readonly ILogger<PostReactionsController> _logger;

        public PostReactionsController(
            ICommunityService communityService,
            ILogger<PostReactionsController> logger)
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

        /// <summary>Add reaction on post</summary>
        [HttpPost]
        public async Task<ActionResult> AddReaction(int postId, [FromBody] AddReactionDto dto)
        {
            try
            {
                var userId = GetCurrentUserId();
                var reaction = await _communityService.AddReactionAsync(postId, userId, dto);
                return Ok(new { success = true, message = "Reaction added successfully", data = reaction });
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
                _logger.LogError(ex, "Error adding reaction on post {PostId}", postId);
                return StatusCode(500, new { success = false, message = "An error occurred" });
            }
        }

        /// <summary>Get all reactions on post</summary>
        [HttpGet]
        public async Task<ActionResult> GetPostReactions(int postId)
        {
            try
            {
                var reactions = await _communityService.GetPostReactionsAsync(postId);
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
                _logger.LogError(ex, "Error retrieving reactions for post {PostId}", postId);
                return StatusCode(500, new { success = false, message = "An error occurred" });
            }
        }

        /// <summary>Get reactions count</summary>
        [HttpGet("count")]
        public async Task<ActionResult> GetReactionsCount(int postId)
        {
            try
            {
                var count = await _communityService.GetReactionsCountAsync(postId);
                return Ok(new { success = true, message = "Reactions count retrieved successfully", data = count });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving reactions count for post {PostId}", postId);
                return StatusCode(500, new { success = false, message = "An error occurred" });
            }
        }

        /// <summary>Update reaction type</summary>
        [HttpPut]
        public async Task<ActionResult> UpdateReaction(int postId, [FromBody] UpdateReactionDto dto)
        {
            try
            {
                var userId = GetCurrentUserId();
                var reaction = await _communityService.UpdateReactionAsync(postId, userId, dto);
                return Ok(new { success = true, message = "Reaction updated successfully", data = reaction });
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
                _logger.LogError(ex, "Error updating reaction on post {PostId}", postId);
                return StatusCode(500, new { success = false, message = "An error occurred" });
            }
        }

        /// <summary>Delete reaction</summary>
        [HttpDelete]
        public async Task<ActionResult> DeleteReaction(int postId)
        {
            try
            {
                var userId = GetCurrentUserId();
                await _communityService.DeleteReactionAsync(postId, userId);
                return Ok(new { success = true, message = "Reaction deleted successfully" });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting reaction on post {PostId}", postId);
                return StatusCode(500, new { success = false, message = "An error occurred" });
            }
        }
    }
}