using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MomEase.core.DTOS.ChatBot;
using MomEase.core.Interfaces;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace PostCare.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    //[Authorize]
    public class ChatBotController : ControllerBase
    {
        private readonly IChatBotService _chatService;

        public ChatBotController(IChatBotService chatService)
        {
            _chatService = chatService;
        }

        /// <summary>
        /// Send a message to the chatbot
        /// </summary>
        [HttpPost("send")]
        public async Task<IActionResult> SendMessage([FromBody] ChatRequestDto request)
        {
            try
            {
                // Extract UserId from JWT token
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                // ✅ TEST MODE FALLBACK (no authentication)
                if (string.IsNullOrEmpty(userIdClaim))
                {
                    if (request.UserId > 0)
                    {
                        userIdClaim = request.UserId.ToString();
                    }
                    else
                    {
                        return Unauthorized(new { message = "User not authenticated" });
                    }
                }

                // Convert UserId to integer
                if (!int.TryParse(userIdClaim, out int userId))
                {
                    return BadRequest(new { message = "Invalid user ID format" });
                }

                // Ensure the UserId in request matches the token
                if (request.UserId != userId)
                {
                    return Forbid(); // 403
                }

                // Send the message
                var response = await _chatService.SendMessageAsync(request);

                return Ok(new
                {
                    success = true,
                    data = response
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new
                {
                    success = false,
                    message = ex.Message
                });
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(503, new
                {
                    success = false,
                    message = "ChatBot service is temporarily unavailable. Please try again later.",
                    details = ex.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "An unexpected error occurred while processing your request",
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
                // Extract UserId from JWT token
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                // ✅ TEST MODE FALLBACK (no authentication)
                if (string.IsNullOrEmpty(userIdClaim))
                {
                    return Unauthorized(new { message = "User not authenticated" });
                }

                if (!int.TryParse(userIdClaim, out int userId))
                {
                    return BadRequest(new { message = "Invalid user ID format" });
                }

                var history = await _chatService.GetChatHistoryAsync(userId);

                return Ok(new
                {
                    success = true,
                    data = history
                });
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new
                {
                    success = false,
                    message = ex.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while fetching chat history",
                    details = ex.Message
                });
            }
        }
    }
}
