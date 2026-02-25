using Microsoft.AspNetCore.Mvc;
using MomEase.core.DTOS.SkinAnalysisDto;
using MomEase.core.Interfaces;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MomEase.api.Controllers
{
    [ApiController]
    [Route("api/diseases")]
    public class DiseasesController : ControllerBase
    {
        private readonly IDiseaseRepository _repo;
        private readonly ILogger<DiseasesController> _logger;

        public DiseasesController(
            IDiseaseRepository repo,
            ILogger<DiseasesController> logger)
        {
            _repo = repo;
            _logger = logger;
        }

        /// <summary>
        /// Get all diseases with medical advice
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                _logger.LogInformation("📋 Fetching all diseases");

                var diseases = await _repo.GetAllAsync();

                var result = diseases.Select(d => new DiseaseDto
                {
                    DiseaseId = d.DiseaseId,
                    Name = d.Name.ToString(),
                    Advice = d.Advice
                }).ToList();

                _logger.LogInformation("✅ Found {Count} diseases", result.Count);

                return Ok(new
                {
                    success = true,
                    count = result.Count,
                    data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error fetching diseases");
                return StatusCode(500, new
                {
                    success = false,
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Get specific disease by ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                _logger.LogInformation("🔍 Fetching disease ID: {Id}", id);

                var disease = await _repo.GetByIdAsync(id);

                if (disease == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = $"Disease with ID {id} not found"
                    });
                }

                return Ok(new
                {
                    success = true,
                    data = new DiseaseDto
                    {
                        DiseaseId = disease.DiseaseId,
                        Name = disease.Name.ToString(),
                        Advice = disease.Advice
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error fetching disease {Id}", id);
                return StatusCode(500, new
                {
                    success = false,
                    error = ex.Message
                });
            }
        }
    }
}
