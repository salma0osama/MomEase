using Google;
using Microsoft.EntityFrameworkCore;
using MomEase.core.Entities;
using MomEase.core.Repositories;
using MomEase.infra.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MomEase.infra.Repositories
{
    public class DeviceTokenRepository : IDeviceTokenRepository
    {
        private readonly MomEaseDbContext _context;

        public DeviceTokenRepository(MomEaseDbContext context)
        {
            _context = context;
        }

        public async Task<DeviceToken> AddOrUpdateTokenAsync(int userId, string token)
        {
            // Check if token already exists
            var existingToken = await _context.DeviceTokens
                .FirstOrDefaultAsync(dt => dt.Token == token);

            if (existingToken != null)
            {
                // Update existing token
                existingToken.UserId = userId;
                existingToken.LastUsedAt = DateTime.Now.AddHours(1);
                existingToken.IsActive = true;

                await _context.SaveChangesAsync();
                return existingToken;
            }

            // Create new token
            var deviceToken = new DeviceToken
            {
                UserId = userId,
                Token = token,
                CreatedAt = DateTime.Now.AddHours(1),
                LastUsedAt = DateTime.Now.AddHours(1),
                IsActive = true
            };

            _context.DeviceTokens.Add(deviceToken);
            await _context.SaveChangesAsync();

            return deviceToken;
        }

        public async Task<bool> RemoveTokenAsync(string token)
        {
            var deviceToken = await _context.DeviceTokens
                .FirstOrDefaultAsync(dt => dt.Token == token);

            if (deviceToken == null)
                return false;

            _context.DeviceTokens.Remove(deviceToken);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> RemoveAllUserTokensAsync(int userId)
        {
            var tokens = await _context.DeviceTokens
                .Where(dt => dt.UserId == userId)
                .ToListAsync();

            if (!tokens.Any())
                return false;

            _context.DeviceTokens.RemoveRange(tokens);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}
