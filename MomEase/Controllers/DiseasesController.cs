using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MomEase.core.DTOS.SkinAnalysisDto;
using MomEase.core.Interfaces;

namespace MomEase.api.Controllers
{
    [ApiController]
    [Route("api/diseases")]
    public class DiseasesController : ControllerBase
    {
        private readonly IDiseaseRepository _repo;

        public DiseasesController(IDiseaseRepository repo)
        {
            _repo = repo;
        }

        /// <summary>
        /// Get all diseases
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var diseases = await _repo.GetAllAsync();
                var result = diseases.Select(d => new DiseaseDto
                {
                    DiseaseId = d.DiseaseId,
                    Name = d.Name.ToString(),
                    Advice = d.Advice
                });
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get disease by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var disease = await _repo.GetByIdAsync(id);
                if (disease == null)
                    return NotFound();

                return Ok(new DiseaseDto
                {
                    DiseaseId = disease.DiseaseId,
                    Name = disease.Name.ToString(),
                    Advice = disease.Advice
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}
