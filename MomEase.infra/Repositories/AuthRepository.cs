using Microsoft.EntityFrameworkCore;
using MomEase.core.Entities;
using MomEase.core.Interfaces;
using MomEase.infra.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MomEase.infra.Repositories
{
    public class AuthRepository : IAuthRepository
    {
        private readonly MomEaseDbContext _context;

        public AuthRepository(MomEaseDbContext context)
        {
            _context = context;
        }

        // User Operations
        public async Task<Users?> GetUserByEmailAsync(string email)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<Users?> GetUserByIdAsync(int userId)
        {
            return await _context.Users
                .FindAsync(userId);
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _context.Users.AnyAsync(u => u.Email == email);
        }

        public async Task AddUserAsync(Users user)
        {
            await _context.Users.AddAsync(user);
        }

        public Task UpdateUserAsync(Users user)
        {
            _context.Users.Update(user);
            return Task.CompletedTask;
        }

        // RefreshToken Operations
        public async Task<RefreshToken?> GetRefreshTokenAsync(string token)
        {
            return await _context.RefreshTokens
                .Include(rt => rt.User)
                .FirstOrDefaultAsync(rt => rt.Token == token);
        }

        public async Task AddRefreshTokenAsync(RefreshToken refreshToken)
        {
            await _context.RefreshTokens.AddAsync(refreshToken);
        }

        public Task UpdateRefreshTokenAsync(RefreshToken refreshToken)
        {
            _context.RefreshTokens.Update(refreshToken);
            return Task.CompletedTask;
        }

        // PasswordReset Operations
        public async Task<PasswordResetToken> GetPasswordResetTokenAsync(string token)
        {
            return await _context.PasswordResetTokens
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.Token == token);
        }
        public async Task<PasswordResetToken> GetPasswordResetTokenByUserIdAsync(int userId, string otpCode)
        {
            return await _context.PasswordResetTokens
                .Include(t => t.User)
                .FirstOrDefaultAsync(t =>
                    t.UserId == userId &&
                    t.Token == otpCode &&
                    t.ExpiresAt > DateTime.Now.AddHours(1) &&
                    !t.IsUsed
                );
        }

        public async Task AddPasswordResetTokenAsync(PasswordResetToken resetToken)
        {
            await _context.PasswordResetTokens.AddAsync(resetToken);
        }

        public Task UpdatePasswordResetTokenAsync(PasswordResetToken resetToken)
        {
            _context.PasswordResetTokens.Update(resetToken);
            return Task.CompletedTask;
        }

        // Save Changes
        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}
