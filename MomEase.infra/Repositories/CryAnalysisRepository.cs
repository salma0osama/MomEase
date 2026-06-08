using Microsoft.EntityFrameworkCore;
using MomEase.core.Entities;
using MomEase.core.Interfaces;
using MomEase.infra.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MomEase.infra.Repositories
{
    public class CryAnalysisRepository : ICryAnalysisRepository
    {
        private readonly MomEaseDbContext _context;

        public CryAnalysisRepository(MomEaseDbContext context)
        {
            _context = context;
        }

        public async Task<CryAnalyses> AddAsync(CryAnalyses analysis)
        {
            _context.CryAnalyses.Add(analysis);
            await _context.SaveChangesAsync();
            return analysis;
        }

        public async Task<CryAnalyses> GetByIdAsync(int id)
        {
            return await _context.CryAnalyses
                .Include(a => a.CryReason)
                .FirstOrDefaultAsync(a => a.CryId == id);
        }

        public async Task<List<CryAnalyses>> GetByUserIdAsync(int userId)
        {
            return await _context.CryAnalyses
                .Include(a => a.CryReason)
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<CryAnalyses>> GetByChildIdAsync(int childId)
        {
            return await _context.CryAnalyses
                .Include(a => a.CryReason)
                .Where(a => a.ChildId == childId)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var analysis = await _context.CryAnalyses.FindAsync(id);
            if (analysis != null)
            {
                _context.CryAnalyses.Remove(analysis);
                await _context.SaveChangesAsync();
            }
        }
    }
}