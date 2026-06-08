using Microsoft.EntityFrameworkCore;
using MomEase.core.Entities;
using MomEase.core.Enums;
using MomEase.core.Interfaces;
using MomEase.infra.Data;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MomEase.infra.Repositories
{
    public class CryReasonsRepository : ICryReasonsRepository
    {
        private readonly MomEaseDbContext _context;

        public CryReasonsRepository(MomEaseDbContext context)
        {
            _context = context;
        }

        public async Task<CryReasons> AddAsync(CryReasons reason)
        {
            _context.CryReasons.Add(reason);
            await _context.SaveChangesAsync();
            return reason;
        }

        public async Task<CryReasons> GetByIdAsync(int id)
        {
            return await _context.CryReasons.FindAsync(id);
        }

        public async Task<CryReasons> GetByNameAsync(CryReasonName name)
        {
            return await _context.CryReasons
                .FirstOrDefaultAsync(r => r.Name == name.ToString());
        }

        public async Task<List<CryReasons>> GetAllAsync()
        {
            return await _context.CryReasons.ToListAsync();
        }

        public async Task<CryReasons> UpdateAsync(CryReasons reason)
        {
            _context.CryReasons.Update(reason);
            await _context.SaveChangesAsync();
            return reason;
        }

        public async Task DeleteAsync(int id)
        {
            var reason = await _context.CryReasons.FindAsync(id);
            if (reason != null)
            {
                _context.CryReasons.Remove(reason);
                await _context.SaveChangesAsync();
            }
        }
    }
}