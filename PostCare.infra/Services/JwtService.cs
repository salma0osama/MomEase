using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using PostCare.core.Entities;
using PostCare.core.Interfaces;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using PostCare.infra.Data;
using Microsoft.EntityFrameworkCore;

namespace PostCare.infra.Services
{
    public class JwtService : IJwtService
    {
        private readonly JwtSettings _jwtSettings;
        private readonly PostCareDbContext _context;

        public JwtService(IOptions<JwtSettings> jwtSettings, PostCareDbContext context)
        {
            _jwtSettings = jwtSettings.Value;
            _context = context;
        }

        public string GenerateAccessToken(Users user)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
                new Claim(ClaimTypes.Role, user.Role.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.Now.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        public async Task<RefreshToken> CreateRefreshTokenAsync(int userId, string token, string ipAddress)
        {
            try
            {
                Console.WriteLine($"[DEBUG] Creating RefreshToken for UserId: {userId}");
                Console.WriteLine($"[DEBUG] Token Length: {token.Length}");
                Console.WriteLine($"[DEBUG] IP Address: {ipAddress}");

                var refreshToken = new RefreshToken
                {
                    UserId = userId,
                    Token = token,
                    ExpiresAt = DateTime.Now.AddDays(_jwtSettings.RefreshTokenExpirationDays),
                    CreatedAt = DateTime.Now,
                    CreatedByIp = ipAddress
                };

                _context.RefreshTokens.Add(refreshToken);

                Console.WriteLine("[DEBUG] Before SaveChanges...");
                await _context.SaveChangesAsync();
                Console.WriteLine("[DEBUG] SaveChanges SUCCESS!");

                return refreshToken;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] CreateRefreshToken failed: {ex.Message}");
                Console.WriteLine($"[ERROR] Inner Exception: {ex.InnerException?.Message}");
                Console.WriteLine($"[ERROR] Stack Trace: {ex.StackTrace}");
                throw;
            }
        }

        public async Task<RefreshToken> GetRefreshTokenAsync(string token)
        {
            return await _context.RefreshTokens
                .Include(rt => rt.User)
                .FirstOrDefaultAsync(rt => rt.Token == token);
        }

        public async Task RevokeRefreshTokenAsync(string token, string ipAddress)
        {
            Console.WriteLine($"[DEBUG] Revoke Token: {token}");
            Console.WriteLine($"[DEBUG] IP Address: {ipAddress ?? "NULL"}");

            var refreshToken = await GetRefreshTokenAsync(token);

            if (refreshToken == null)
            {
                Console.WriteLine("[DEBUG] RefreshToken not found!");
                return;
            }

            Console.WriteLine($"[DEBUG] Token found, IsActive: {refreshToken.IsActive}");

            refreshToken.RevokedAt = DateTime.Now;
            refreshToken.RevokedByIp = ipAddress ?? "Unknown";

            await _context.SaveChangesAsync();
            Console.WriteLine("[DEBUG] Token revoked successfully!");
        }

        public async Task<bool> ValidateRefreshTokenAsync(string token)
        {
            var refreshToken = await GetRefreshTokenAsync(token);
            return refreshToken != null && refreshToken.IsActive;
        }
    }
}
