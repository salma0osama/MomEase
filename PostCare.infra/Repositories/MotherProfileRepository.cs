using Microsoft.EntityFrameworkCore;
using PostCare.core.Entities;
using PostCare.core.Interfaces;
using PostCare.infra.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PostCare.infra.Repositories
{
    /// <summary>
    /// Repository implementation for Mother Profile
    /// </summary>
    public class MotherProfileRepository : IMotherProfileRepository
    {
        private readonly PostCareDbContext _context;

        public MotherProfileRepository(PostCareDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<MotherProfile?> GetByIdAsync(int motherId)
        {
            return await _context.MotherProfiles
                .FirstOrDefaultAsync(m => m.MotherId == motherId);
        }

        public async Task<MotherProfile?> GetByUserIdAsync(int userId)
        {
            return await _context.MotherProfiles
                .FirstOrDefaultAsync(m => m.UserId == userId);
        }

        public async Task<MotherProfile?> GetByUserIdWithUserAsync(int userId)
        {
            return await _context.MotherProfiles
                .Include(m => m.User)
                .FirstOrDefaultAsync(m => m.UserId == userId);
        }

        public async Task<bool> ExistsByUserIdAsync(int userId)
        {
            return await _context.MotherProfiles
                .AnyAsync(m => m.UserId == userId);
        }

        public async Task<MotherProfile> AddAsync(MotherProfile motherProfile)
        {
            if (motherProfile == null)
                throw new ArgumentNullException(nameof(motherProfile));

            await _context.MotherProfiles.AddAsync(motherProfile);
            return motherProfile;
        }

        public Task UpdateAsync(MotherProfile motherProfile)
        {
            if (motherProfile == null)
                throw new ArgumentNullException(nameof(motherProfile));

            _context.MotherProfiles.Update(motherProfile);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(MotherProfile motherProfile)
        {
            if (motherProfile == null)
                throw new ArgumentNullException(nameof(motherProfile));

            _context.MotherProfiles.Remove(motherProfile);
            return Task.CompletedTask;
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}
