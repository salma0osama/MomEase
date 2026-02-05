using MomEase.core.DTOS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MomEase.core.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto, string ipAddress);
        Task<AuthResponseDto> LoginAsync(LoginDto loginDto, string ipAddress);
        Task<AuthResponseDto> RefreshTokenAsync(string refreshToken, string ipAddress);
        Task<bool> RevokeTokenAsync(string refreshToken, string ipAddress);
        Task<bool> ChangePasswordAsync(int userId, ChangePasswordDto changePasswordDto);
        Task LogoutAsync(string refreshToken, string ipAddress);
        Task<string> ForgotPasswordAsync(string email);
        Task ResetPasswordAsync(string otpCode, string email, string newPassword);

        Task<bool> VerifyEmailAsync(VerifyEmailDto verifyEmailDto);
        Task<bool> ResendOtpAsync(string email);
        Task<AuthResponseDto> GoogleLoginAsync(GoogleLoginDto googleLoginDto, string ipAddress);
    }
}
