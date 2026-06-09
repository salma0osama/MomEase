using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MomEase.core.DTOS.ChatBot;
using MomEase.core.Interfaces;
using System.Security.Claims;

namespace PostCare.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatBotController : ControllerBase
    {
        private readonly IChatBotService _chatService;
        private readonly ILogger<ChatBotController> _logger;

        public ChatBotController(IChatBotService chatService, ILogger<ChatBotController> logger)
        {
            _chatService = chatService ?? throw new ArgumentNullException(nameof(chatService));
            _logger = logger;
        }

        /// <summary>
        /// Send a message to the chatbot
        /// </summary>
        [HttpPost("send")]
        public async Task<IActionResult> SendMessage([FromBody] ChatRequestDto request)
        {
            try
            {
                // ✅ Extract UserId from JWT or use from request (for testing)
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userIdClaim))
                {
                    if (request?.UserId > 0)
                    {
                        userIdClaim = request.UserId.ToString();
                    }
                    else
                    {
                        return Unauthorized(new { message = "User not authenticated" });
                    }
                }

                if (!int.TryParse(userIdClaim, out int userId))
                    return BadRequest(new { message = "Invalid user ID format" });

                // ✅ Verify userId matches
                if (request.UserId != userId)
                    return Forbid();

                _logger.LogInformation($"📨 Message from user {userId}: {request.Message.Substring(0, Math.Min(30, request.Message.Length))}...");

                var response = await _chatService.SendMessageAsync(request);

                return Ok(new
                {
                    success = true,
                    data = response
                });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Validation error");
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Operation failed");
                return StatusCode(503, new
                {
                    success = false,
                    message = "ChatBot service is temporarily unavailable. Make sure Ollama is running!",
                    details = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error");
                return StatusCode(500, new
                {
                    success = false,
                    message = "An unexpected error occurred",
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// Get chat history for a user
        /// </summary>
        [HttpGet("history")]
        public async Task<IActionResult> GetHistory()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userIdClaim))
                    return Unauthorized(new { message = "User not authenticated" });

                if (!int.TryParse(userIdClaim, out int userId))
                    return BadRequest(new { message = "Invalid user ID format" });

                _logger.LogInformation($"📖 Fetching history for user {userId}");

                var history = await _chatService.GetChatHistoryAsync(userId);

                return Ok(new
                {
                    success = true,
                    data = history
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching chat history");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Failed to retrieve chat history",
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// Delete a chat
        /// </summary>
        [HttpDelete("delete/{chatId}")]
        public async Task<IActionResult> DeleteChat(int chatId)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (!int.TryParse(userIdClaim, out int userId))
                    return Unauthorized();

                _logger.LogInformation($"🗑️ Deleting chat {chatId} for user {userId}");

                var result = await _chatService.DeleteChatAsync(userId, chatId);

                if (result)
                    return Ok(new { success = true, message = "Chat deleted successfully" });
                else
                    return NotFound(new { success = false, message = "Chat not found" });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting chat");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
    }
}