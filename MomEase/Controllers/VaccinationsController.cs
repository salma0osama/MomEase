using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using Microsoft.AspNetCore.Authorization;
using MomEase.core.DTOS.VaccinationDTO;
using MomEase.core.Interfaces;
using System.Security.Claims;

namespace MomEase.api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class VaccinationsController : ControllerBase
    {
        private readonly IVaccinationService _vaccinationService;
        private readonly ILogger<VaccinationsController> _logger;

        public VaccinationsController(
            IVaccinationService vaccinationService,
            ILogger<VaccinationsController> logger)
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

        /// <summary>Get all vaccinations</summary>
        [HttpGet]
        public async Task<ActionResult> GetAllVaccinations()
        {
            try
            {
                var vaccinations = await _vaccinationService.GetAllVaccinationsAsync();
                return Ok(new
                {
                    success = true,
                    message = "Vaccinations retrieved successfully",
                    count = vaccinations.Count,
                    data = vaccinations
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all vaccinations");
                return StatusCode(500, new { success = false, message = "An error occurred" });
            }
        }

        /// <summary>Get vaccination by ID</summary>
        [HttpGet("{id}")]
        public async Task<ActionResult> GetVaccinationById(int id)
        {
            try
            {
                var vaccination = await _vaccinationService.GetVaccinationByIdAsync(id);
                return Ok(new { success = true, message = "Vaccination retrieved successfully", data = vaccination });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving vaccination {Id}", id);
                return StatusCode(500, new { success = false, message = "An error occurred" });
            }
        }

        /// <summary>Get vaccinations by age in months</summary>
        [HttpGet("by-age/{age}")]
        public async Task<ActionResult> GetVaccinationsByAge(int age)
        {
            try
            {
                var vaccinations = await _vaccinationService.GetVaccinationsByAgeAsync(age);
                return Ok(new
                {
                    success = true,
                    message = "Vaccinations retrieved successfully",
                    count = vaccinations.Count,
                    data = vaccinations
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving vaccinations for age {Age}", age);
                return StatusCode(500, new { success = false, message = "An error occurred" });
            }
        }
    }
}