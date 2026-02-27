using Azure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using MomEase.core.Entities;
using MomEase.core.Interfaces;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MomEase.api.Controllers
{
    [ApiController]
    [Route("api/assessment-results/{resultId:int}/responses")]
    [Authorize]
    public class UserResponsesController : ControllerBase
    {
        private readonly IUserResponseService _service;

        public UserResponsesController(IUserResponseService service)
        {
            _service = service;
        }

        /// <summary>
        /// Get all responses for a specific assessment result
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAllResponses(int resultId)
        {
            try
            {
                // Get UserId from JWT token
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userIdClaim))
                {
                    return Unauthorized(new { message = "User not authenticated" });
                }

                if (!int.TryParse(userIdClaim, out int userId))
                {
                    return BadRequest(new { message = "Invalid user ID format" });
                }

                var responses = await _service.GetResponsesByResultIdAsync(userId, resultId);

                return Ok(new
                {
                    success = true,
                    data = responses
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
                    message = "An error occurred while fetching responses",
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// Get a specific response by ID
        /// </summary>
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetResponseById(int resultId, int id)
        {
            try
            {
                // Get UserId from JWT token
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userIdClaim))
                {
                    return Unauthorized(new { message = "User not authenticated" });
                }

                if (!int.TryParse(userIdClaim, out int userId))
                {
                    return BadRequest(new { message = "Invalid user ID format" });
                }

                var response = await _service.GetResponseByIdAsync(userId, resultId, id);

                if (response == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = $"Response {id} not found in result {resultId}"
                    });
                }

                return Ok(new
                {
                    success = true,
                    data = response
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while fetching the response",
                    details = ex.Message
                });
            }
        }
    }
}

