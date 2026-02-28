using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using Microsoft.AspNetCore.Authorization;
using MomEase.core.DTOS.VaccinationDTO;
using MomEase.core.Interfaces;
using System.Security.Claims;

namespace MomEase.api.Controllers
{
    [ApiController]
    [Route("api/children")]
    [Authorize(Roles = "MOTHER")]
    public class ChildVaccinationController : ControllerBase
    {
        private readonly IVaccinationService _vaccinationService;
        private readonly ILogger<ChildVaccinationController> _logger;

        public ChildVaccinationController(
            IVaccinationService vaccinationService,
            ILogger<ChildVaccinationController> logger)
        {
            _vaccinationService = vaccinationService;
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

        /// <summary>Add vaccination to child</summary>
        [HttpPost("{childId}/vaccinations")]
        public async Task<ActionResult> AddChildVaccination(int childId, [FromBody] int scheduleId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _vaccinationService.AddChildVaccinationAsync(childId, userId, scheduleId);
                return CreatedAtAction(
                    nameof(GetChildVaccinationById),
                    new { childId, id = result.ChildVaccineId },
                    new { success = true, message = "Vaccination added successfully", data = result });
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
                _logger.LogError(ex, "Error adding vaccination to child {ChildId}", childId);
                return StatusCode(500, new { success = false, message = "An error occurred" });
            }
        }

        /// <summary>Get all vaccinations for a child</summary>
        [HttpGet("{childId}/vaccinations")]
        public async Task<ActionResult> GetChildVaccinations(int childId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var vaccinations = await _vaccinationService
                    .GetChildVaccinationsGroupedAsync(childId, userId);

                return Ok(new
                {
                    success = true,
                    message = "Vaccinations retrieved successfully",
                    count = vaccinations.Count,
                    data = vaccinations
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving vaccinations for child {ChildId}", childId);
                return StatusCode(500, new { success = false, message = "An error occurred" });
            }
        }

        /// <summary>Get specific vaccination by ID</summary>
        [HttpGet("{childId}/vaccinations/{id}")]
        public async Task<ActionResult> GetChildVaccinationById(int childId, int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                var vaccination = await _vaccinationService
                    .GetChildVaccinationByIdAsync(id, childId, userId);

                return Ok(new { success = true, message = "Vaccination retrieved successfully", data = vaccination });
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
                _logger.LogError(ex, "Error retrieving vaccination {Id}", id);
                return StatusCode(500, new { success = false, message = "An error occurred" });
            }
        }

        /// <summary>Update vaccination status</summary>
        [HttpPut("{childId}/vaccinations/{id}")]
        public async Task<ActionResult> UpdateVaccinationStatus(
            int childId, int id, [FromBody] UpdateVaccinationStatusDto dto)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _vaccinationService
                    .UpdateVaccinationStatusAsync(id, childId, userId, dto);

                return Ok(new { success = true, message = "Vaccination status updated successfully", data = result });
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
                _logger.LogError(ex, "Error updating vaccination status {Id}", id);
                return StatusCode(500, new { success = false, message = "An error occurred" });
            }
        }

        /// <summary>Delete vaccination</summary>
        [HttpDelete("{childId}/vaccinations/{id}")]
        public async Task<ActionResult> DeleteChildVaccination(int childId, int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                await _vaccinationService.DeleteChildVaccinationAsync(id, childId, userId);

                return Ok(new { success = true, message = "Vaccination deleted successfully" });
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
                _logger.LogError(ex, "Error deleting vaccination {Id}", id);
                return StatusCode(500, new { success = false, message = "An error occurred" });
            }
        }

        /// <summary>Get upcoming vaccinations</summary>
        [HttpGet("{childId}/vaccinations/upcoming")]
        public async Task<ActionResult> GetUpcomingVaccinations(
            int childId, [FromQuery] int daysAhead = 30)
        {
            try
            {
                var userId = GetCurrentUserId();
                var upcoming = await _vaccinationService
                    .GetUpcomingVaccinationsAsync(childId, userId, daysAhead);

                return Ok(new
                {
                    success = true,
                    message = "Upcoming vaccinations retrieved successfully",
                    count = upcoming.Count,
                    data = upcoming
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving upcoming vaccinations for child {ChildId}", childId);
                return StatusCode(500, new { success = false, message = "An error occurred" });
            }
        }

        /// <summary>Get overdue vaccinations</summary>
        [HttpGet("{childId}/vaccinations/overdue")]
        public async Task<ActionResult> GetOverdueVaccinations(int childId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var overdue = await _vaccinationService.GetOverdueVaccinationsAsync(childId, userId);

                return Ok(new
                {
                    success = true,
                    message = "Overdue vaccinations retrieved successfully",
                    count = overdue.Count,
                    data = overdue
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving overdue vaccinations for child {ChildId}", childId);
                return StatusCode(500, new { success = false, message = "An error occurred" });
            }
        }

        /// <summary>Get completed vaccinations</summary>
        [HttpGet("{childId}/vaccinations/completed")]
        public async Task<ActionResult> GetCompletedVaccinations(int childId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var completed = await _vaccinationService.GetCompletedVaccinationsAsync(childId, userId);

                return Ok(new
                {
                    success = true,
                    message = "Completed vaccinations retrieved successfully",
                    count = completed.Count,
                    data = completed
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving completed vaccinations for child {ChildId}", childId);
                return StatusCode(500, new { success = false, message = "An error occurred" });
            }
        }

        /// <summary>Mark vaccination as taken</summary>
        [HttpPut("{childId}/vaccinations/{id}/mark-taken")]
        public async Task<ActionResult> MarkVaccinationAsTaken(
            int childId, int id, [FromBody] UpdateVaccinationStatusDto dto)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _vaccinationService
                    .MarkVaccinationAsTakenAsync(id, childId, userId, dto);

                return Ok(new { success = true, message = "Vaccination marked as taken successfully", data = result });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { success = false, message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking vaccination as taken {Id}", id);
                return StatusCode(500, new { success = false, message = "An error occurred" });
            }
        }
    }
}