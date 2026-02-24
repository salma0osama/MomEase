using Google;
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
    public class SkinAnalysisRepository : ISkinAnalysisRepository
    {
        private readonly MomEaseDbContext _context;

        public SkinAnalysisRepository(MomEaseDbContext context)
        {
            _context = context;
        }

        public async Task<SkinAnalyses> AddAsync(SkinAnalyses analysis)
        {
            _context.SkinAnalyses.Add(analysis);
            await _context.SaveChangesAsync();
            return analysis;
        }

        public async Task<SkinAnalyses> GetByIdAsync(int id)
        {
            return await _context.SkinAnalyses
                .Include(a => a.Disease)
                .FirstOrDefaultAsync(a => a.SkinanalysisId == id);
        }

        public async Task<List<SkinAnalyses>> GetByUserIdAsync(int userId)
        {
            return await _context.SkinAnalyses
                .Include(a => a.Disease)
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<SkinAnalyses>> GetByChildIdAsync(int childId)
        {
            return await _context.SkinAnalyses
                .Include(a => a.Disease)
                .Where(a => a.ChildId == childId)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var analysis = await _context.SkinAnalyses.FindAsync(id);
            if (analysis != null)
            {
                _context.SkinAnalyses.Remove(analysis);
                await _context.SaveChangesAsync();
            }
        }
    }
}
