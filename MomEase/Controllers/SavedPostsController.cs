using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MomEase.core.Interfaces;
using System.Security.Claims;

[ApiController]
[Route("api/community")]
[Authorize]
public class SavedPostsController : ControllerBase
{
    private readonly ICommunityService _communityService;
    private readonly ILogger<SavedPostsController> _logger;

    public SavedPostsController(
        ICommunityService communityService,
        ILogger<SavedPostsController> logger)
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

    /// <summary>Save a post</summary>
    [HttpPost("posts/{postId}/save")]
    public async Task<ActionResult> SavePost(int postId)
    {
        try
        {
            var userId = GetCurrentUserId();
            var saved = await _communityService.SavePostAsync(postId, userId);
            return Ok(new
            {
                success = true,
                message = "Post saved successfully",
                data = saved
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving post {PostId}", postId);
            return StatusCode(500, new { success = false, message = "An error occurred" });
        }
    }

    /// <summary>Remove post from saved</summary>
    [HttpDelete("posts/{postId}/save")]
    public async Task<ActionResult> RemoveFromSaved(int postId)
    {
        try
        {
            var userId = GetCurrentUserId();
            await _communityService.RemoveFromSavedAsync(postId, userId);
            return Ok(new { success = true, message = "Post removed from saved successfully" });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing saved post {PostId}", postId);
            return StatusCode(500, new { success = false, message = "An error occurred" });
        }
    }

    /// <summary>Get my saved posts</summary>
    [HttpGet("saved-posts")]
    public async Task<ActionResult> GetSavedPosts()
    {
        try
        {
            var userId = GetCurrentUserId();
            var savedPosts = await _communityService.GetSavedPostsAsync(userId);
            return Ok(new
            {
                success = true,
                message = "Saved posts retrieved successfully",
                count = savedPosts.Count,
                data = savedPosts
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving saved posts");
            return StatusCode(500, new { success = false, message = "An error occurred" });
        }
    }
}