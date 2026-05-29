using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MomEase.core.DTOS.CommunityDTO;
using MomEase.core.Interfaces;
using System.Security.Claims;

namespace MomEase.api.Controllers
{
    [ApiController]
    [Route("api/community/posts")]
    [Authorize]
    public class CommunityPostsController : ControllerBase
    {
        private readonly ICommunityService _communityService;
        private readonly ILogger<CommunityPostsController> _logger;

        public CommunityPostsController(
            ICommunityService communityService,
            ILogger<CommunityPostsController> logger)
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

        /// <summary>Get all posts with pagination</summary>
        [HttpGet]
        public async Task<ActionResult> GetAllPosts(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var userId = GetCurrentUserId();
                var posts = await _communityService.GetAllPostsAsync(pageNumber, pageSize, userId);
                return Ok(new
                {
                    success = true,
                    message = "Posts retrieved successfully",
                    data = posts
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving posts");
                return StatusCode(500, new { success = false, message = "An error occurred" });
            }
        }

        /// <summary>Get post by ID</summary>
        [HttpGet("{id}")]
        public async Task<ActionResult> GetPostById(int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                var post = await _communityService.GetPostByIdAsync(id, userId);
                return Ok(new { success = true, message = "Post retrieved successfully", data = post });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving post {Id}", id);
                return StatusCode(500, new { success = false, message = "An error occurred" });
            }
        }

        /// <summary>Create new post</summary>
        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult> CreatePost([FromForm] CreatePostDto dto)
        {
            try
            {
                var userId = GetCurrentUserId();
                var post = await _communityService.CreatePostAsync(userId, dto);
                return CreatedAtAction(
                    nameof(GetPostById),
                    new { id = post.PostId },
                    new { success = true, message = "Post created successfully", data = post });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating post");
                return StatusCode(500, new
                {
                    success = false,
                    message = ex.Message,
                    inner = ex.InnerException?.Message,
                    trace = ex.StackTrace?.Split('\n').Take(5)
                });
            }
        }

        /// <summary>Update post</summary>
        [HttpPut("{id}")]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult> UpdatePost(int id, [FromForm] UpdatePostDto dto)
        {
            try
            {
                var userId = GetCurrentUserId();
                var post = await _communityService.UpdatePostAsync(id, userId, dto);
                return Ok(new { success = true, message = "Post updated successfully", data = post });
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
                _logger.LogError(ex, "Error updating post {Id}", id);
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>Delete post</summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeletePost(int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                // تأكد إن الـ Role بالظبط زي اللي في الـ JWT
                var isAdmin = User.IsInRole("ADMIN") || User.HasClaim("role", "ADMIN");
                await _communityService.DeletePostAsync(id, userId, isAdmin);
                return Ok(new { success = true, message = "Post deleted successfully" });
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
                _logger.LogError(ex, "Error deleting post {Id}", id);
                return StatusCode(500, new { success = false, message = "An error occurred" });
            }
        }

        /// <summary>Get posts by user ID</summary>
        [HttpGet("user/{userId}")]
        public async Task<ActionResult> GetPostsByUserId(
            int userId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var posts = await _communityService
                    .GetPostsByUserIdAsync(userId, pageNumber, pageSize, currentUserId);
                return Ok(new
                {
                    success = true,
                    message = "Posts retrieved successfully",
                    data = posts
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving posts for user {UserId}", userId);
                return StatusCode(500, new { success = false, message = "An error occurred" });
            }
        }

        /// <summary>Get my posts</summary>
        [HttpGet("my-posts")]
        public async Task<ActionResult> GetMyPosts(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var userId = GetCurrentUserId();
                var posts = await _communityService.GetMyPostsAsync(userId, pageNumber, pageSize);
                return Ok(new
                {
                    success = true,
                    message = "Posts retrieved successfully",
                    data = posts
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving my posts");
                return StatusCode(500, new { success = false, message = "An error occurred" });
            }
        }
    }
}