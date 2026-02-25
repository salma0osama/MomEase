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
    public class AssessmentRepository : IAssessmentRepository
    {
        private readonly MomEaseDbContext _context;

        public AssessmentRepository(MomEaseDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Assessment>> GetAllAsync()
        {
            return await _context.Assessments
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Assessment?> GetByIdAsync(int id)
        {
            return await _context.Assessments
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.AssessmentId == id);
        }

        public async Task<Assessment> CreateAsync(Assessment assessment)
        {
            _context.Assessments.Add(assessment);
            await _context.SaveChangesAsync();
            return assessment;
        }

        public async Task<Assessment> UpdateAsync(Assessment assessment)
        {
            _context.Assessments.Update(assessment);
            await _context.SaveChangesAsync();
            return assessment;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var assessment = await _context.Assessments.FindAsync(id);
            if (assessment == null) return false;

            _context.Assessments.Remove(assessment);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Assessments.AnyAsync(a => a.AssessmentId == id);
        }
    }
}
