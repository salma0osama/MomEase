using Google;
using Microsoft.EntityFrameworkCore;
using MomEase.core.Entities;
using MomEase.core.Enums;
using MomEase.core.Interfaces;
using MomEase.infra.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MomEase.infra.Repositories
{
    public class DiseaseRepository : IDiseaseRepository
    {
        private readonly MomEaseDbContext _context;

        public DiseaseRepository(MomEaseDbContext context)
        {
            _context = context;
        }

        public async Task<List<Diseases>> GetAllAsync()
        {
            return await _context.Diseases.ToListAsync();
        }

        public async Task<Diseases> GetByIdAsync(int id)
        {
            return await _context.Diseases.FindAsync(id);
        }

        public async Task<Diseases> GetByNameAsync(SkinAnalysisDiseaseName name)
        {
            return await _context.Diseases
                .FirstOrDefaultAsync(d => d.Name == name);
        }
    }
}
