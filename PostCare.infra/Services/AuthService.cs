using PostCare.core.DTOS;
using PostCare.core.Entities;
using PostCare.core.Enums;
using PostCare.core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;
using PostCare.infra.Data;
using PostCare.infra.Data;


namespace PostCare.infra.Services
{
    public class AuthService : IAuthService
    {
        private readonly PostCareDbContext _context;
        private readonly IJwtService _jwtService;

        public AuthService(PostCareDbContext context, IJwtService jwtService)
        {
            _context = context;
            _jwtService = jwtService;
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto, string ipAddress)
        {
            // Check if email already exists
            if (await _context.Users.AnyAsync(u => u.Email == registerDto.Email))
                throw new Exception("Email already exists");

            // Hash password
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(registerDto.Password);

            // Create user
            var user = new Users
            {
                FirstName = registerDto.FirstName,
                LastName = registerDto.LastName,
                Email = registerDto.Email,
                Password = hashedPassword,
                Phone = registerDto.Phone,
                Age = registerDto.Age,
                Role = Role.MOTHER,
                CreatedAt = DateTime.Now
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Create Mother Profile
            var motherProfile = new MotherProfile
            {
                UserId = user.UserId,
                IsFirstTimeMother = true,
                NumberOfChildren = 0
            };

            _context.MotherProfiles.Add(motherProfile);
            await _context.SaveChangesAsync();

            // Generate tokens
            var accessToken = _jwtService.GenerateAccessToken(user);
            var refreshToken = _jwtService.GenerateRefreshToken();
            await _jwtService.CreateRefreshTokenAsync(user.UserId, refreshToken, ipAddress);

            return new AuthResponseDto
            {
                UserId = user.UserId,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Role = user.Role.ToString(),
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                AccessTokenExpiration = DateTime.Now.AddMinutes(120),
                RefreshTokenExpiration = DateTime.Now.AddDays(14)
            };
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto loginDto, string ipAddress)
        {
            // Find user
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == loginDto.Email);

            if (user == null)
                throw new Exception("Invalid email or password");

            // Verify password
            if (!BCrypt.Net.BCrypt.Verify(loginDto.Password, user.Password))
                throw new Exception("Invalid email or password");

            // Generate tokens
            var accessToken = _jwtService.GenerateAccessToken(user);
            var refreshToken = _jwtService.GenerateRefreshToken();
            await _jwtService.CreateRefreshTokenAsync(user.UserId, refreshToken, ipAddress);

            return new AuthResponseDto
            {
                UserId = user.UserId,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Role = user.Role.ToString(),
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                AccessTokenExpiration = DateTime.Now.AddMinutes(120),
                RefreshTokenExpiration = DateTime.Now.AddDays(14)
            };
        }

        public async Task<AuthResponseDto> RefreshTokenAsync(string refreshToken, string ipAddress)
        {
            var token = await _jwtService.GetRefreshTokenAsync(refreshToken);

            if (token == null || !token.IsActive)
                throw new Exception("Invalid refresh token");

            // Revoke old token
            await _jwtService.RevokeRefreshTokenAsync(refreshToken, ipAddress);

            // Generate new tokens
            var user = token.User;
            var newAccessToken = _jwtService.GenerateAccessToken(user);
            var newRefreshToken = _jwtService.GenerateRefreshToken();
            await _jwtService.CreateRefreshTokenAsync(user.UserId, newRefreshToken, ipAddress);

            return new AuthResponseDto
            {
                UserId = user.UserId,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Role = user.Role.ToString(),
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken,
                AccessTokenExpiration = DateTime.Now.AddMinutes(120),
                RefreshTokenExpiration = DateTime.Now.AddDays(14)
            };
        }

        public async Task<bool> RevokeTokenAsync(string refreshToken, string ipAddress)
        {
            await _jwtService.RevokeRefreshTokenAsync(refreshToken, ipAddress);
            return true;
        }

        public async Task<bool> ChangePasswordAsync(int userId, ChangePasswordDto changePasswordDto)
        {
            var user = await _context.Users.FindAsync(userId);

            if (user == null)
                throw new Exception("User not found");

            // Verify current password
            if (!BCrypt.Net.BCrypt.Verify(changePasswordDto.CurrentPassword, user.Password))
                throw new Exception("Current password is incorrect");

            // Hash and update new password
            user.Password = BCrypt.Net.BCrypt.HashPassword(changePasswordDto.NewPassword);

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return true;
        }
        public async Task LogoutAsync(string refreshToken, string ipAddress)
        {
            var token = await _context.RefreshTokens
        .FirstOrDefaultAsync(t => t.Token == refreshToken
                                  && t.RevokedAt == null               // ميكنش اتلغى قبل كدا
                                  && t.ExpiresAt > DateTime.Now);
            if (token == null)
                throw new Exception("Invalid token");

            // Revoke the token
            token.RevokedAt = DateTime.Now;
            token.RevokedByIp = ipAddress;

            await _context.SaveChangesAsync();
        }
        
        public async Task<string> ForgotPasswordAsync(string email)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
                throw new Exception("User not found");

            // توليد توكن فريد بطريقة آمنة وبسيطة
            var resetToken = Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N");
            // "N" بتطلع نص مكون من حروف وأرقام فقط بدون شرط (32 حرف)

            var passwordResetToken = new PasswordResetToken
            {
                UserId = user.UserId,
                Token = resetToken,
                ExpiresAt = DateTime.Now.AddHours(1),
                CreatedAt = DateTime.Now
            };

            _context.PasswordResetTokens.Add(passwordResetToken);
            await _context.SaveChangesAsync();

            return resetToken;
        }

        public async Task ResetPasswordAsync(string token, string newPassword)
        {
            var resetToken = await _context.PasswordResetTokens
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.Token == token);

            if (resetToken == null)
                throw new Exception("Invalid reset token");

            if (!resetToken.IsValid)
                throw new Exception("Reset token has expired or already been used");

            // Hash the new password
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);

            // Update user password
            resetToken.User.Password = passwordHash;

            // Mark token as used
            resetToken.IsUsed = true;
            resetToken.UsedAt = DateTime.Now;

            await _context.SaveChangesAsync();
        }
    }
}
