using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MomEase.core.DTOS.AssessmentDto;
using MomEase.core.Interfaces;
using static MomEase.api.Filters.SwaggerLanguageHeaderFilter;
namespace MomEase.api.Controllers
{
    [ApiController]
    [Route("api/assessments")]
    public class AssessmentsController : ControllerBase
    {
        private readonly IAssessmentService _service;

        public AssessmentsController(IAssessmentService service)
        {
            _service = service;
        }

        // GET /api/assessments
        [HttpGet]
        [LocalizedEndpoint]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAllAsync();
            return Ok(result);
        }

        // GET /api/assessments/{id}  → metadata فقط
        [HttpGet("{id:int}")]
        [LocalizedEndpoint]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _service.GetByIdAsync(id);
            if (result == null)
                return NotFound(new { message = $"Assessment {id} not found." });

            return Ok(result);
        }

        // POST /api/assessments  [ADMIN]
        [HttpPost]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Create([FromBody] CreateAssessmentDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var created = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.AssessmentId }, created);
        }

        // PUT /api/assessments/{id}  [ADMIN]
        [HttpPut("{id:int}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateAssessmentDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var updated = await _service.UpdateAsync(id, dto);
            if (updated == null)
                return NotFound(new { message = $"Assessment {id} not found." });

            return Ok(updated);
        }

        // DELETE /api/assessments/{id}  [ADMIN]
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _service.DeleteAsync(id);
            if (!deleted)
                return NotFound(new { message = $"Assessment {id} not found." });

            return Ok(new { message = "Assessment removed successfully" });
        }
    }
}
