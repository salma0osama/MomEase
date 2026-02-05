using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MomEase.core.DTOS.MotherProfileDto;
using MomEase.core.Interfaces;
using System.Security.Claims;

namespace MomEase.api.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class MotherProfileController : ControllerBase
    {
        private readonly IMotherProfileService _motherProfileService;

        public MotherProfileController(IMotherProfileService motherProfileService)
        {
            _motherProfileService = motherProfileService ?? throw new ArgumentNullException(nameof(motherProfileService));
        }

        /// <summary>
        /// POST /api/mother-profile - إنشاء بروفايل الأم
        /// (يتم استدعاؤه تلقائياً أثناء التسجيل - لكن متاح للاستخدام اليدوي)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "MOTHER")]
        public async Task<IActionResult> CreateMotherProfile([FromBody] CreateMotherProfileDto createDto)
        {
            try
            {
                // Validate that the userId matches the logged-in user
                var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

                if (createDto.UserId != currentUserId)
                {
                    return Forbid(); // User can only create profile for themselves
                }

                var profile = await _motherProfileService.CreateMotherProfileAsync(createDto);

                return Ok(new
                {
                    success = true,
                    message = "Mother profile created successfully",
                    data = profile
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while creating mother profile",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// GET /api/mother-profile - الحصول على بروفايل الأم للمستخدم الحالي
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "MOTHER")]
        public async Task<IActionResult> GetMyMotherProfile()
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var profile = await _motherProfileService.GetMyProfileAsync(userId);

                return Ok(new
                {
                    success = true,
                    message = "Mother profile retrieved successfully",
                    data = profile
                });
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new
                {
                    success = false,
                    message = ex.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while retrieving mother profile",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// GET /api/mother-profile/{id} - الحصول على بروفايل أم معينة
        /// (للأدمن فقط - من خلال User ID)
        /// </summary>
        [HttpGet("{id}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> GetMotherProfileById(int id)
        {
            try
            {
                // Get current admin user id from token
                var currentUserId = int.Parse(
                    User.FindFirst(ClaimTypes.NameIdentifier)!.Value
                );

                // 🚫 Prevent admin from requesting their own profile BEFORE DB call
                if (id == currentUserId)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Admins do not have a mother profile to view."
                    });
                }

                // ✅ Safe to fetch profile now
                var profile = await _motherProfileService.GetMotherProfileByIdAsync(id);

                return Ok(new
                {
                    success = true,
                    message = "Mother profile retrieved successfully",
                    data = profile
                });
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new
                {
                    success = false,
                    message = ex.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while retrieving mother profile",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// PUT /api/mother-profile - تحديث بروفايل الأم
        /// </summary>
        [HttpPut]
        [Authorize(Roles = "MOTHER")]
        public async Task<IActionResult> UpdateMotherProfile([FromBody] UpdateMotherProfileDto updateDto)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var updatedProfile = await _motherProfileService.UpdateMotherProfileAsync(userId, updateDto);

                return Ok(new
                {
                    success = true,
                    message = "Mother profile updated successfully",
                    data = updatedProfile
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while updating mother profile",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// DELETE /api/mother-profile - حذف بروفايل الأم
        /// </summary>
        [HttpDelete]
        [Authorize(Roles = "MOTHER")]
        public async Task<IActionResult> DeleteMotherProfile()
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var result = await _motherProfileService.DeleteMotherProfileAsync(userId);

                if (!result)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Failed to delete mother profile"
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = "Mother profile deleted successfully"
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while deleting mother profile",
                    error = ex.Message
                });
            }
        }
    }
}
