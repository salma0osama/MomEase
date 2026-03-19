using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MomEase.core.DTOS.CreateAnswerOptionsDto;
using MomEase.core.Interfaces;
using static MomEase.api.Filters.SwaggerLanguageHeaderFilter;
namespace MomEase.api.Controllers
{
    [ApiController]
    [Route("api/questions/{questionId:int}/options")]
    public class AnswerOptionsController : ControllerBase
    {
        private readonly IAnswerOptionService _service;

        public AnswerOptionsController(IAnswerOptionService service)
        {
            _service = service;
        }

        // GET /api/questions/{questionId}/options
        [HttpGet]
        [LocalizedEndpoint]
        public async Task<IActionResult> GetAll(int questionId)
        {
            var result = await _service.GetAllByQuestionAsync(questionId);
            return Ok(result);
        }

        // GET /api/questions/{questionId}/options/{id}
        [HttpGet("{id:int}")]
        [LocalizedEndpoint]
        public async Task<IActionResult> GetById(int questionId, int id)
        {
            var result = await _service.GetByIdAsync(questionId, id);
            if (result == null)
                return NotFound(new { message = $"Option {id} not found in Question {questionId}." });

            return Ok(result);
        }

        // POST /api/questions/{questionId}/options  [ADMIN]
        [HttpPost]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Create(int questionId, [FromBody] CreateAnswerOptionDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var (result, error) = await _service.CreateAsync(questionId, dto);
            if (error != null)
                return NotFound(new { message = error });

            return CreatedAtAction(nameof(GetById), new { questionId, id = result!.OptionId }, result);
        }

        // PUT /api/questions/{questionId}/options/{id}  [ADMIN]
        [HttpPut("{id:int}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Update(int questionId, int id, [FromBody] UpdateAnswerOptionDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var (result, error) = await _service.UpdateAsync(questionId, id, dto);
            if (error != null)
                return NotFound(new { message = error });

            return Ok(result);
        }

        // DELETE /api/questions/{questionId}/options/{id}  [ADMIN]
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Delete(int questionId, int id)
        {
            var deleted = await _service.DeleteAsync(questionId, id);
            if (!deleted)
                return NotFound(new { message = $"Option {id} not found in Question {questionId}." });

            return Ok(new { message = "Option removed successfully" });
        }
    }
}
