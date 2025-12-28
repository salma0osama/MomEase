using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PostCare.core.DTOS;
using PostCare.core.Interfaces;
using System.Security.Claims;

namespace PostCare.api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            try
            {
                // استخرج الـ UserId من الـ Token
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

                // اجلب البيانات من الـ Service
                var profile = await _userService.GetUserProfileAsync(userId);

                return Ok(new
                {
                    success = true,
                    message = "Profile retrieved successfully",
                    data = profile
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }
        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateUserProfileDto updateDto)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

                var updatedProfile = await _userService.UpdateUserProfileAsync(userId, updateDto);

                return Ok(new
                {
                    success = true,
                    message = "Profile updated successfully",
                    data = updatedProfile
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }
        [HttpDelete("account")]
        public async Task<IActionResult> DeleteAccount()
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

                // احذف الحساب
                var result = await _userService.DeleteUserAccountAsync(userId);

                if (!result)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Failed to delete account"
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = "Account deleted successfully"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }
        [HttpPut("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto changePasswordDto)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

                await _userService.ChangePasswordAsync(userId, changePasswordDto);

                return Ok(new
                {
                    success = true,
                    message = "Password changed successfully"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }
        [HttpGet("profile/stats")]
        public async Task<IActionResult> GetProfileStats()
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

                var profile = await _userService.GetUserByIdAsync(userId);

                return Ok(new
                {
                    success = true,
                    message = "Stats retrieved successfully",
                    data = new
                    {
                        userId = profile.UserId,
                        childrenCount = profile.ChildrenCount,
                        hasMotherProfile = profile.HasMotherProfile,
                        memberSince = profile.CreatedAt
                    }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        
    }
}


