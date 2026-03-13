using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MomEase.core.Interfaces;
using System.Security.Claims;

namespace MomEase.api.Controllers
{
    [ApiController]
    [Route("api/notifications")]
    [Authorize]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationService _service;

        public NotificationsController(INotificationService service)
        {
            _service = service;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                throw new UnauthorizedAccessException("User ID not found");
            return userId;
        }

        /// <summary>
        /// Get all notifications for current user
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var userId = GetCurrentUserId();
                var notifications = await _service.GetUserNotificationsAsync(userId);

                return Ok(new { success = true, data = notifications });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Get unread notifications count
        /// </summary>
        [HttpGet("unread-count")]
        public async Task<IActionResult> GetUnreadCount()
        {
            try
            {
                var userId = GetCurrentUserId();
                var count = await _service.GetUnreadCountAsync(userId);

                return Ok(new { success = true, count });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Mark notification as read
        /// </summary>
        [HttpPut("{id}/read")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _service.MarkAsReadAsync(userId, id);

                if (!result)
                    return NotFound(new { success = false, message = "Notification not found" });

                return Ok(new { success = true, message = "Notification marked as read" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Mark all notifications as read
        /// </summary>
        [HttpPut("mark-all-read")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            try
            {
                var userId = GetCurrentUserId();
                await _service.MarkAllAsReadAsync(userId);

                return Ok(new { success = true, message = "All notifications marked as read" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Delete notification
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _service.DeleteNotificationAsync(userId, id);

                if (!result)
                    return NotFound(new { success = false, message = "Notification not found" });

                return Ok(new { success = true, message = "Notification deleted" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
    }
}
