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
using PostCare.core.DTOS.MotherProfileDto;


namespace PostCare.infra.Services
{
    public class AuthService : IAuthService
    {
        private readonly IAuthRepository _authRepository;
        private readonly IJwtService _jwtService;
        private readonly IEmailService _emailService;
        private readonly IMotherProfileService _motherProfileService;

        public AuthService(
            IAuthRepository authRepository,
            IJwtService jwtService,
            IEmailService emailService,IMotherProfileService motherProfileService)
            
        {
            _authRepository = authRepository;
            _jwtService = jwtService;
            _emailService = emailService;
            _motherProfileService=motherProfileService;
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto, string ipAddress)
        {
            // Check if email exists
            if (await _authRepository.EmailExistsAsync(registerDto.Email))
                throw new Exception("Email already exists");

            // Hash password
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(registerDto.Password);
            var otpCode = GenerateOtpCode();

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
                CreatedAt = DateTime.Now,
                IsEmailVerified = false,
                EmailVerificationToken = otpCode,
                EmailVerificationTokenExpiry = DateTime.Now.AddMinutes(1)
            };

            await _authRepository.AddUserAsync(user);
            await _authRepository.SaveChangesAsync();

            // Create Mother Profile automatically
            await _motherProfileService.CreateMotherProfileAsync(new CreateMotherProfileDto
            {
                UserId = user.UserId,
                IsFirstTimeMother = true,
                NumberOfChildren = 0
            });

            // Send OTP Email
            await _emailService.SendOtpEmailAsync(
                user.Email,
                otpCode,
                $"{user.FirstName} {user.LastName}"
            );

           
            return new AuthResponseDto
            {
                UserId = user.UserId,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Role = user.Role.ToString(),
                AccessToken = null,
                RefreshToken = null,
                AccessTokenExpiration = null,
                RefreshTokenExpiration = null
            };
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto loginDto, string ipAddress)
        {
            // Find user
            var user = await _authRepository.GetUserByEmailAsync(loginDto.Email);

            if (user == null)
                throw new Exception("Invalid email or password");

            // Verify password
            if (!BCrypt.Net.BCrypt.Verify(loginDto.Password, user.Password))
                throw new Exception("Invalid email or password");

            // Check email verification
            if (!user.IsEmailVerified)
                throw new Exception("Please verify your email before logging in.");

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

        public async Task<bool> VerifyEmailAsync(VerifyEmailDto verifyEmailDto)
        {
            var user = await _authRepository.GetUserByEmailAsync(verifyEmailDto.Email);

            if (user == null)
                throw new Exception("User not found");

            if (user.IsEmailVerified)
                throw new Exception("Email already verified");

            if (user.EmailVerificationToken != verifyEmailDto.OtpCode)
                throw new Exception("Invalid OTP code");

            if (user.EmailVerificationTokenExpiry < DateTime.Now)
                throw new Exception("OTP code has expired. Please request a new one.");

            // Mark as verified
            user.IsEmailVerified = true;
            user.EmailVerificationToken = null;
            user.EmailVerificationTokenExpiry = null;

            await _authRepository.UpdateUserAsync(user);
            await _authRepository.SaveChangesAsync();

            return true;
        }

        public async Task<bool> ResendOtpAsync(string email)
        {
            var user = await _authRepository.GetUserByEmailAsync(email);

            if (user == null)
                throw new Exception("User not found");

            if (user.IsEmailVerified)
                throw new Exception("Email already verified");

            // Generate new OTP
            var otpCode = GenerateOtpCode();
            user.EmailVerificationToken = otpCode;
            user.EmailVerificationTokenExpiry = DateTime.Now.AddMinutes(1);

            await _authRepository.UpdateUserAsync(user);
            await _authRepository.SaveChangesAsync();

            // Send new OTP
            await _emailService.SendOtpEmailAsync(
                user.Email,
                otpCode,
                $"{user.FirstName} {user.LastName}"
            );

            return true;
        }

        public async Task<AuthResponseDto> RefreshTokenAsync(string refreshToken, string ipAddress)
        {
            var token = await _authRepository.GetRefreshTokenAsync(refreshToken);

            if (token == null || !token.IsActive)
                throw new Exception("Invalid refresh token");

            // Revoke old token
            token.RevokedAt = DateTime.Now;
            token.RevokedByIp = ipAddress ?? "Unknown";
            await _authRepository.UpdateRefreshTokenAsync(token);
            await _authRepository.SaveChangesAsync();

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
            var token = await _authRepository.GetRefreshTokenAsync(refreshToken);

            if (token == null)
                return false;

            token.RevokedAt = DateTime.Now;
            token.RevokedByIp = ipAddress ?? "Unknown";

            await _authRepository.UpdateRefreshTokenAsync(token);
            await _authRepository.SaveChangesAsync();

            return true;
        }

        public async Task<bool> ChangePasswordAsync(int userId, ChangePasswordDto changePasswordDto)
        {
            var user = await _authRepository.GetUserByIdAsync(userId);

            if (user == null)
                throw new Exception("User not found");

            // Verify current password
            if (!BCrypt.Net.BCrypt.Verify(changePasswordDto.CurrentPassword, user.Password))
                throw new Exception("Current password is incorrect");

            // Hash and update new password
            user.Password = BCrypt.Net.BCrypt.HashPassword(changePasswordDto.NewPassword);

            await _authRepository.UpdateUserAsync(user);
            await _authRepository.SaveChangesAsync();

            return true;
        }

        public async Task LogoutAsync(string refreshToken, string ipAddress)
        {
            var token = await _authRepository.GetRefreshTokenAsync(refreshToken);

            if (token == null || token.RevokedAt != null || token.ExpiresAt <= DateTime.Now)
                throw new Exception("Invalid token");

            token.RevokedAt = DateTime.Now;
            token.RevokedByIp = ipAddress ?? "Unknown";

            await _authRepository.UpdateRefreshTokenAsync(token);
            await _authRepository.SaveChangesAsync();
        }

        public async Task<string> ForgotPasswordAsync(string email)
        {
            var user = await _authRepository.GetUserByEmailAsync(email);

            if (user == null)
                throw new Exception("User not found");

            
            var otpCode = GenerateOtpCode();

            var passwordResetToken = new PasswordResetToken
            {
                UserId = user.UserId,
                Token = otpCode, 
                ExpiresAt = DateTime.Now.AddMinutes(10), // 10 دقايق
                CreatedAt = DateTime.Now
            };

            await _authRepository.AddPasswordResetTokenAsync(passwordResetToken);
            await _authRepository.SaveChangesAsync();

            // ابعت OTP email
            await _emailService.SendPasswordResetOtpEmailAsync(
                user.Email,
                otpCode,
                $"{user.FirstName} {user.LastName}"
            );

            return otpCode; // في الـ production متحطوش في الـ response
        }

        public async Task ResetPasswordAsync(string otpCode, string email, string newPassword)
        {
            // جيب اليوزر الأول
            var user = await _authRepository.GetUserByEmailAsync(email);

            if (user == null)
                throw new Exception("User not found");

            // جيب الـ token بتاع اليوزر ده
            var resetToken = await _authRepository.GetPasswordResetTokenByUserIdAsync(user.UserId, otpCode);

            if (resetToken == null)
                throw new Exception("Invalid OTP code");

            if (!resetToken.IsValid)
                throw new Exception("OTP has expired or already been used");

            // Update password
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);

            user.Password = passwordHash;
            resetToken.IsUsed = true;
            resetToken.UsedAt = DateTime.Now;

            await _authRepository.UpdateUserAsync(user);
            await _authRepository.UpdatePasswordResetTokenAsync(resetToken);
            await _authRepository.SaveChangesAsync();
        }

        //public async Task ResetPasswordAsync(string otpCode, string email, string newPassword)
        //{
        //    var user = await _authRepository.GetUserByEmailAsync(email);

        //    if (user == null)
        //        throw new Exception("User not found");

        //    var resetToken = await _authRepository.GetPasswordResetTokenByUserIdAsync(user.UserId, otpCode);

        //    if (resetToken == null)
        //        throw new Exception("Invalid OTP code");

        //    if (!resetToken.IsValid)
        //        throw new Exception("OTP has expired or already been used");

        //    var passwordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);

        //    user.Password = passwordHash;
        //    resetToken.IsUsed = true;
        //    resetToken.UsedAt = DateTime.Now;

        //    await _authRepository.UpdateUserAsync(user);
        //    await _authRepository.UpdatePasswordResetTokenAsync(resetToken);
        //    await _authRepository.SaveChangesAsync();
        //}

        // Helper Method
        private string GenerateOtpCode()
        {
            var random = new Random();
            return random.Next(1000, 9999).ToString();
        }
    }
}