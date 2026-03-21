using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MomEase.core.DTOS.SkinAnalysisDto;
using MomEase.core.Entities;
using MomEase.core.Interfaces;
using System.Security.Claims;

[ApiController]
[Route("api/skin-analysis")]
[Authorize]
public class SkinAnalysisController : ControllerBase
{
    private readonly ISkinAnalysisService _skinAnalysisService;
    private readonly ISkinAnalysisAIService _aiService;
    private readonly IChildRepository _childRepo;
    private readonly ILogger<SkinAnalysisController> _logger;

    public SkinAnalysisController(
        ISkinAnalysisService skinAnalysisService,
        ISkinAnalysisAIService aiService,
        IChildRepository childRepo,
        ILogger<SkinAnalysisController> logger)
    {
        _skinAnalysisService = skinAnalysisService;
        _aiService = aiService;
        _childRepo = childRepo;
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

    [HttpPost("analyze")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> AnalyzeImage(
      [FromForm] SkinAnalysisRequestDto request,
      IFormFile image)
    {
        try
        {
            var userId = GetCurrentUserId();

            if (request.ChildId.HasValue)
            {
                var isOwned = await _childRepo
                    .IsChildOwnedByUserAsync(request.ChildId.Value, userId);
                if (!isOwned)
                    throw new KeyNotFoundException("Child not found");
            }

            var result = await _skinAnalysisService
                .AnalyzeNewImageAsync(userId, request, image);

            return Ok(new
            {
                success = true,
                message = "Image analyzed successfully",
                data = result
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
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
            _logger.LogError(ex, "Error analyzing image");
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }
    [HttpGet("user")]
    public async Task<IActionResult> GetUserAnalyses()
    {
        try
        {
            var userId = GetCurrentUserId();
            var analyses = await _skinAnalysisService.GetUserAnalysesAsync(userId);
            return Ok(new
            {
                success = true,
                message = "Analyses retrieved successfully",
                count = analyses.Count,
                data = analyses
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user analyses");
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }

    [HttpGet("child/{childId}")]
    public async Task<IActionResult> GetChildAnalyses(int childId)
    {
        try
        {
            var userId = GetCurrentUserId();
            var isOwned = await _childRepo.IsChildOwnedByUserAsync(childId, userId);
            if (!isOwned)
                return NotFound(new { success = false, message = "Child not found" });

            var analyses = await _skinAnalysisService.GetChildAnalysesAsync(childId);
            return Ok(new
            {
                success = true,
                message = "Analyses retrieved successfully",
                count = analyses.Count,
                data = analyses
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving child analyses");
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetAnalysisById(int id)
    {
        try
        {
            var analysis = await _skinAnalysisService.GetAnalysisByIdAsync(id);
            return Ok(new
            {
                success = true,
                message = "Analysis retrieved successfully",
                data = analysis
            });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving analysis {Id}", id);
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAnalysis(int id)
    {
        try
        {
            await _skinAnalysisService.DeleteAnalysisAsync(id);
            return Ok(new { success = true, message = "Analysis deleted successfully" });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting analysis {Id}", id);
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }

    [HttpGet("diseases")]
    public async Task<IActionResult> GetAllDiseases()
    {
        try
        {
            var diseases = await _skinAnalysisService.GetAllDiseasesAsync();
            return Ok(new
            {
                success = true,
                message = "Diseases retrieved successfully",
                count = diseases.Count,
                data = diseases
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving diseases");
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }

    [HttpGet("diseases/{id}")]
    public async Task<IActionResult> GetDiseaseById(int id)
    {
        try
        {
            var disease = await _skinAnalysisService.GetDiseaseByIdAsync(id);
            return Ok(new
            {
                success = true,
                message = "Disease retrieved successfully",
                data = disease
            });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving disease {Id}", id);
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }
}