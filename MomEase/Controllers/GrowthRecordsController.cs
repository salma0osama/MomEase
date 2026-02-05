using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MomEase.core.DTOS.GrowthTracking;
using MomEase.core.Interfaces;
using System.Security.Claims;

namespace MomEase.api.Controllers
{
    [Route("api/children/{childId}/growth-records")]
    [ApiController]
    [Authorize]
    public class GrowthRecordsController : ControllerBase
    {
        private readonly IGrowthRecordService _service;

        public GrowthRecordsController(IGrowthRecordService service)
        {
            _service = service;
        }
        // POST /api/children/5/growth-records
        [HttpPost]
        public async Task<IActionResult> Create(int childId, [FromBody] CreateGrowthRecordDto dto)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var result = await _service.CreateAsync(childId, userId, dto);

                return Ok(new { success = true, message = "Record created", data = result });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
        // GET /api/children/5/growth-records
        [HttpGet]
        public async Task<IActionResult> GetAll(int childId)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var result = await _service.GetAllAsync(childId, userId);

                return Ok(new { success = true, data = result });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
        // GET /api/children/5/growth-records/10
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int childId, int id)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var result = await _service.GetByIdAsync(id, childId, userId);

                return Ok(new { success = true, data = result });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        // PUT /api/children/5/growth-records/10
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int childId, int id, [FromBody] UpdateGrowthRecordDto dto)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var result = await _service.UpdateAsync(id, childId, userId, dto);

                return Ok(new { success = true, message = "Record updated", data = result });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        // DELETE /api/children/5/growth-records/10
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int childId, int id)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var result = await _service.DeleteAsync(id, childId, userId);

                return Ok(new { success = true, message = "Record deleted" });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        // GET /api/children/5/growth-records/chart
        [HttpGet("chart")]
        public async Task<IActionResult> GetChart(int childId)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var result = await _service.GetChartDataAsync(childId, userId);

                return Ok(new { success = true, data = result });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }
}
