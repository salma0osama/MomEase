using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MomEase.core.DTOS.DeviceTokenDto;
using MomEase.core.Repositories;
using System.Security.Claims;

namespace MomEase.api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/device-tokens")]
    public class DeviceTokensController : ControllerBase
    {
        private readonly IDeviceTokenRepository _deviceTokenRepository;

        public DeviceTokensController(IDeviceTokenRepository deviceTokenRepository)
        {
            _deviceTokenRepository = deviceTokenRepository;
        }

        /// <summary>
        /// Register or update device token for Android push notifications
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> RegisterDeviceToken([FromBody] RegisterDeviceTokenDto dto)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                    return Unauthorized(new { message = "Invalid user token" });

                var token = await _deviceTokenRepository.AddOrUpdateTokenAsync(
                    userId,
                    dto.DeviceToken);

                return Ok(new
                {
                    success = true,
                    message = "Device token registered successfully",
                    tokenId = token.TokenId
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Failed to register device token",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Remove device token (on logout)
        /// </summary>
        [HttpDelete]
        public async Task<IActionResult> RemoveDeviceToken([FromBody] RegisterDeviceTokenDto dto)
        {
            try
            {
                var removed = await _deviceTokenRepository.RemoveTokenAsync(dto.DeviceToken);

                if (!removed)
                    return NotFound(new { message = "Device token not found" });

                return Ok(new
                {
                    success = true,
                    message = "Device token removed successfully"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Failed to remove device token",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Remove all device tokens for current user
        /// </summary>
        [HttpDelete("all")]
        public async Task<IActionResult> RemoveAllDeviceTokens()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                    return Unauthorized(new { message = "Invalid user token" });

                var removed = await _deviceTokenRepository.RemoveAllUserTokensAsync(userId);

                if (!removed)
                    return NotFound(new { message = "No device tokens found" });

                return Ok(new
                {
                    success = true,
                    message = "All device tokens removed successfully"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Failed to remove device tokens",
                    error = ex.Message
                });
            }
        }
    }
}
