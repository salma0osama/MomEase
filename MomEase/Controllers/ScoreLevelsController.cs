using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MomEase.core.DTOS.AssessmentDto;
using MomEase.core.Interfaces;
using System;
using System.Threading.Tasks;

namespace MomEase.api.Controllers
{
    [ApiController]
    [Route("api/assessments/{assessmentId:int}/score-levels")]
    public class ScoreLevelsController : ControllerBase
    {
        private readonly IScoreLevelService _service;

        public ScoreLevelsController(IScoreLevelService service)
        {
            _service = service;
        }

        /// <summary>
        /// Get all score levels for an assessment
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll(int assessmentId)
        {
            try
            {
                var result = await _service.GetAllByAssessmentAsync(assessmentId);
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
                    message = "An error occurred while fetching score levels",
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// Get a specific score level by ID
        /// </summary>
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int assessmentId, int id)
        {
            try
            {
                var result = await _service.GetByIdAsync(assessmentId, id);

                if (result == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = $"Score level {id} not found in assessment {assessmentId}"
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
                    message = "An error occurred while fetching the score level",
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// Create a new score level (Admin only)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Create(int assessmentId, [FromBody] CreateScoreLevelDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Invalid input data",
                        errors = ModelState
                    });
                }

                var (result, error) = await _service.CreateAsync(assessmentId, dto);

                if (error != null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = error
                    });
                }

                return CreatedAtAction(
                    nameof(GetById),
                    new { assessmentId, id = result!.LevelId },
                    new
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
                    message = "An error occurred while creating the score level",
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// Update an existing score level (Admin only)
        /// </summary>
        [HttpPut("{id:int}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Update(int assessmentId, int id, [FromBody] UpdateScoreLevelDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Invalid input data",
                        errors = ModelState
                    });
                }

                var (result, error) = await _service.UpdateAsync(assessmentId, id, dto);

                if (error != null)
                {
                    return NotFound(new
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
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while updating the score level",
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// Delete a score level (Admin only)
        /// </summary>
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Delete(int assessmentId, int id)
        {
            try
            {
                var deleted = await _service.DeleteAsync(assessmentId, id);

                if (!deleted)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = $"Score level {id} not found in assessment {assessmentId}"
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = "Score level deleted successfully"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while deleting the score level",
                    details = ex.Message
                });
            }
        }
    }
}