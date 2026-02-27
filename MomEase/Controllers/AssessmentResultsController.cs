using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MomEase.core.DTOS.AssessmentDto;
using MomEase.core.Interfaces;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MomEase.api.Controllers
{
    [ApiController]
    [Route("api/assessments/{assessmentId:int}")]
    [Authorize]
    public class AssessmentResultsController : ControllerBase
    {
        private readonly IAssessmentResultService _service;

        public AssessmentResultsController(IAssessmentResultService service)
        {
            _service = service;
        }

        /// <summary>
        /// Submit assessment answers and get results
        /// </summary>
        [HttpPost("submit")]
        public async Task<IActionResult> Submit(int assessmentId, [FromBody] SubmitAssessmentDto dto)
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

                if (!ModelState.IsValid)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Invalid input data",
                        errors = ModelState
                    });
                }

                var (result, error) = await _service.SubmitAssessmentAsync(userId, assessmentId, dto);

                if (error != null)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = error
                    });
                }

                return Ok(new
                {
                    success = true,
                    data = result
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
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while submitting the assessment",
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// Get all assessment results for the authenticated user
        /// </summary>
        [HttpGet("/api/assessment-results")]
        public async Task<IActionResult> GetAllResults()
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

                var results = await _service.GetUserResultsAsync(userId);

                return Ok(new
                {
                    success = true,
                    data = results
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while fetching assessment results",
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// Get a specific assessment result by ID
        /// </summary>
        [HttpGet("/api/assessment-results/{id:int}")]
        public async Task<IActionResult> GetResultById(int id)
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

                var result = await _service.GetResultByIdAsync(userId, id);

                if (result == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = $"Assessment result {id} not found"
                    });
                }

                return Ok(new
                {
                    success = true,
                    data = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while fetching the assessment result",
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// Get detailed assessment result with all responses
        /// </summary>
        [HttpGet("/api/assessment-results/{id:int}/details")]
        public async Task<IActionResult> GetResultDetails(int id)
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

                var result = await _service.GetResultDetailsAsync(userId, id);

                if (result == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = $"Assessment result {id} not found"
                    });
                }

                return Ok(new
                {
                    success = true,
                    data = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while fetching result details",
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// Delete an assessment result
        /// </summary>
        [HttpDelete("/api/assessment-results/{id:int}")]
        public async Task<IActionResult> DeleteResult(int id)
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

                var deleted = await _service.DeleteResultAsync(userId, id);

                if (!deleted)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = $"Assessment result {id} not found"
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = "Assessment result deleted successfully"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while deleting the result",
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// Get latest assessment result (for quick access)
        /// </summary>
        [HttpGet("/api/assessment-results/latest")]
        public async Task<IActionResult> GetLatestResult()
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

                var result = await _service.GetLatestResultAsync(userId);

                if (result == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "No assessment results found"
                    });
                }

                return Ok(new
                {
                    success = true,
                    data = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while fetching the latest result",
                    details = ex.Message
                });
            }
        }
    }
}