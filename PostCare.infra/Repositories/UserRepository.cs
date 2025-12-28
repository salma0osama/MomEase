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
    public class UserRepository : IUserRepository
    {
        private readonly PostCareDbContext _context;

        public UserRepository(PostCareDbContext context)
        {
            _context = context;
        }

        public async Task<Users> GetByIdAsync(int userId)
        {
            return await _context.Users
                .Include(u => u.MotherProfile)
                .Include(u => u.Children)
                .FirstOrDefaultAsync(u => u.UserId == userId);
        }

        public async Task<Users> GetByEmailAsync(string email)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<IEnumerable<Users>> GetAllAsync()
        {
            return await _context.Users
                .Include(u => u.MotherProfile)
                .Include(u => u.Children)
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync();
        }

        public async Task<Users> UpdateAsync(Users user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<bool> DeleteAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return false;

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(int userId)
        {
            return await _context.Users.AnyAsync(u => u.UserId == userId);
        }

        public async Task<int> GetChildrenCountAsync(int userId)
        {
            return await _context.Children
                .CountAsync(c => c.UserId == userId);
        }

        public async Task<bool> HasMotherProfileAsync(int userId)
        {
            return await _context.MotherProfiles
                .AnyAsync(m => m.UserId == userId);
        }
    }
}
