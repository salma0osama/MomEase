using PostCare.core.DTOS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PostCare.core.Interfaces
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
        Task ResetPasswordAsync(string token, string newPassword);
    }
}
