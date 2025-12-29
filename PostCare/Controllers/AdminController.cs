using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PostCare.core.DTOS.AdminDTO;
using PostCare.core.DTOS;
using PostCare.core.Interfaces;
using System.Security.Claims;
using PostCare.infra.Services;
using PostCare.core.Enums;
namespace PostCare.api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "ADMIN")]
    public class AdminController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IMotherProfileService _motherProfileService;

        public AdminController(IUserService userService, IMotherProfileService motherProfileService)
        {
            _userService = userService;
            _motherProfileService = motherProfileService;
        }

        //Admin-only endpoints
        [HttpGet("users/{id}")]
        public async Task<IActionResult> GetUserById(int id)
        {
            try
            {
                var user = await _userService.GetUserByIdAsync(id);

                return Ok(new
                {
                    success = true,
                    message = "User retrieved successfully",
                    data = user
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
        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers()
        {
            try
            {
                var users = await _userService.GetAllUsersAsync();

                return Ok(new
                {
                    success = true,
                    message = "Users retrieved successfully",
                    count = users.Count(),
                    data = users
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
        [HttpPut("users/{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserProfileDto updateDto)
        {
            try
            {
                var updatedUser = await _userService.UpdateUserAsync(id, updateDto);

                return Ok(new
                {
                    success = true,
                    message = "User updated successfully",
                    data = updatedUser
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
        [HttpDelete("users/{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            try
            {
                // تأكد أن الأدمن لا يحذف نفسه
                var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

                if (currentUserId == id)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Admin cannot delete their own account"
                    });
                }

                var result = await _userService.DeleteUserAsync(id);

                if (!result)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Failed to delete user"
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = "User deleted successfully"
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
        // ============================================
        // 5. PUT /api/admin/users/{id}/role
        // تغيير دور المستخدم (MOTHER/ADMIN)
        // ============================================
        [HttpPut("users/{id}/role")]
        public async Task<IActionResult> ChangeUserRole(int id, [FromBody] ChangeRoleDto changeRoleDto)
        {
            try
            {
                // TODO: سنضيف هذه الوظيفة لاحقاً في Service
                return Ok(new
                {
                    success = true,
                    message = "User role updated successfully (Feature coming soon)"
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


        //// ============================================
        //// 6. PUT /api/admin/users/{id}/status
        //// تفعيل/تعطيل المستخدم
        //// ============================================
        [HttpPut("users/{id}/status")]
        public async Task<IActionResult> ChangeUserStatus(int id, [FromBody] ChangeStatusDto changeStatusDto)
        {
            try
            {
                // TODO: سنضيف هذه الوظيفة لاحقاً في Service
                return Ok(new
                {
                    success = true,
                    message = $"User status changed successfully (Feature coming soon)"
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


        // ============================================
        // 7. GET /api/admin/content-moderation
        // إدارة المحتوى المبلغ عنه
        // ============================================
        [HttpGet("content-moderation")]
        public async Task<IActionResult> GetReportedContent()
        {
            try
            {
                // TODO: سنضيف هذه الوظيفة عند عمل Community Module
                return Ok(new
                {
                    success = true,
                    message = "Content moderation retrieved successfully",
                    data = new List<object>() // placeholder
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

        // ============================================
        // 8. GET /api/admin/system-logs
        // سجلات النظام
        // ============================================
        [HttpGet("system-logs")]
        public async Task<IActionResult> GetSystemLogs()
        {
            try
            {
                // TODO: سنضيف Logging System لاحقاً
                return Ok(new
                {
                    success = true,
                    message = "System logs retrieved successfully",
                    data = new List<object>() // placeholder
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

