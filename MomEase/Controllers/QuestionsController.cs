using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MomEase.core.DTOS.QuestionsDto;
using MomEase.core.Interfaces;
using static MomEase.api.Filters.SwaggerLanguageHeaderFilter;
namespace MomEase.api.Controllers
{
    [ApiController]
    [Route("api/assessments/{assessmentId:int}/questions")]
    public class QuestionsController : ControllerBase
    {
        private readonly IQuestionService _service;

        public QuestionsController(IQuestionService service)
        {
            _service = service;
        }

        // GET /api/assessments/{assessmentId}/questions
        [HttpGet]
        [LocalizedEndpoint]
        public async Task<IActionResult> GetAll(int assessmentId)
        {
            var result = await _service.GetAllByAssessmentAsync(assessmentId);
            return Ok(result);
        }

        // GET /api/assessments/{assessmentId}/questions/{id}
        [HttpGet("{id:int}")]
        [LocalizedEndpoint]
        public async Task<IActionResult> GetById(int assessmentId, int id)
        {
            var result = await _service.GetByIdAsync(assessmentId, id);
            if (result == null)
                return NotFound(new { message = $"Question {id} not found in Assessment {assessmentId}." });

            return Ok(result);
        }

        // POST /api/assessments/{assessmentId}/questions  [ADMIN]
        [HttpPost]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Create(int assessmentId, [FromBody] CreateQuestionDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var (result, error) = await _service.CreateAsync(assessmentId, dto);
            if (error != null)
                return NotFound(new { message = error });

            return CreatedAtAction(nameof(GetById), new { assessmentId, id = result!.QuestionId }, result);
        }

        // PUT /api/assessments/{assessmentId}/questions/{id}  [ADMIN]
        [HttpPut("{id:int}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Update(int assessmentId, int id, [FromBody] UpdateQuestionDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var (result, error) = await _service.UpdateAsync(assessmentId, id, dto);
            if (error != null)
                return NotFound(new { message = error });

            return Ok(result);
        }

        // DELETE /api/assessments/{assessmentId}/questions/{id}  [ADMIN]
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Delete(int assessmentId, int id)
        {
            var deleted = await _service.DeleteAsync(assessmentId, id);
            if (!deleted)
                return NotFound(new { message = $"Question {id} not found in Assessment {assessmentId}." });

            return Ok(new { message = "Question removed successfully" });

        }
    }
}
