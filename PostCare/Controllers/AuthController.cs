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
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            try
            {
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                var result = await _authService.RegisterAsync(registerDto, ipAddress);

                return Ok(new
                {
                    success = true,
                    message = "Registration successful",
                    data = result
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

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            try
            {
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                var result = await _authService.LoginAsync(loginDto, ipAddress);

                return Ok(new
                {
                    success = true,
                    message = "Login successful",
                    data = result
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

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenDto refreshTokenDto)
        {
            try
            {
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                var result = await _authService.RefreshTokenAsync(refreshTokenDto.RefreshToken, ipAddress);

                return Ok(new
                {
                    success = true,
                    message = "Token refreshed successfully",
                    data = result
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

        //[Authorize]
        [HttpPost("revoke-token")]
        public async Task<IActionResult> RevokeToken([FromBody] RefreshTokenDto refreshTokenDto)
        {
            try
            {
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
                await _authService.RevokeTokenAsync(refreshTokenDto.RefreshToken, ipAddress);

                return Ok(new
                {
                    success = true,
                    message = "Token revoked successfully"
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

        [Authorize]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto changePasswordDto)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                await _authService.ChangePasswordAsync(userId, changePasswordDto);

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

        [Authorize]
        [HttpGet("me")]
        public IActionResult GetCurrentUser()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            var name = User.FindFirst(ClaimTypes.Name)?.Value;
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            return Ok(new
            {
                success = true,
                data = new
                {
                    userId,
                    email,
                    name,
                    role
                }
            });
        }
        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] LogoutDto logoutDto)
        {
            try
            {
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
                await _authService.LogoutAsync(logoutDto.RefreshToken, ipAddress);

                return Ok(new
                {
                    success = true,
                    message = "Logged out successfully"
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
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto forgotPasswordDto)
        {
            try
            {
                var resetToken = await _authService.ForgotPasswordAsync(forgotPasswordDto.Email);

                return Ok(new
                {
                    success = true,
                    message = "A password reset code has been sent to your email.",
                    // في الإنتاج لا ترسل الـ token في الـ response
                    // أرسله فقط عبر الإيميل
                    data = new { resetToken } // Remove this in production
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
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto resetPasswordDto)
        {
            try
            {
                await _authService.ResetPasswordAsync(
                    resetPasswordDto.OtpCode,
                    resetPasswordDto.Email,
                    resetPasswordDto.NewPassword
                );

                return Ok(new
                {
                    success = true,
                    message = "Password has been reset successfully"
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
        [HttpPost("verify-email")]
        public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailDto verifyEmailDto)
        {
            try
            {
                await _authService.VerifyEmailAsync(verifyEmailDto);

                return Ok(new
                {
                    success = true,
                    message = "Email verified successfully! You can now login."
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

        [HttpPost("resend-otp")]
        public async Task<IActionResult> ResendOtp([FromBody] ResendOtpDto resendOtpDto)
        {
            try
            {
                await _authService.ResendOtpAsync(resendOtpDto.Email);

                return Ok(new
                {
                    success = true,
                    message = "OTP code sent successfully to your email"
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
