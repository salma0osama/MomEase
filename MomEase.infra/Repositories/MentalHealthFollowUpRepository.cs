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
    public class MentalHealthFollowUpRepository : IMentalHealthFollowUpRepository
    {
        private readonly MomEaseDbContext _context;

        public MentalHealthFollowUpRepository(MomEaseDbContext context)
        {
            _context = context;
        }

        public async Task<MentalHealthFollowUp> CreateAsync(MentalHealthFollowUp followUp)
        {
            _context.MentalHealthFollowUps.Add(followUp);
            await _context.SaveChangesAsync();
            return followUp;
        }

        public async Task<MentalHealthFollowUp?> GetActiveByUserIdAsync(int userId)
        {
            return await _context.MentalHealthFollowUps
                .Where(f => f.UserId == userId && !f.IsCompleted)
                .OrderByDescending(f => f.CreatedAt)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<MentalHealthFollowUp>> GetDueAssessmentRemindersAsync()
        {
            var now = DateTime.Now;
            return await _context.MentalHealthFollowUps
                .Where(f => !f.IsCompleted
                         && !f.AssessmentReminderSent
                         && f.NextAssessmentDate <= now)
                .ToListAsync();
        }

        public async Task<IEnumerable<MentalHealthFollowUp>> GetDueTipsAsync()
        {
            var now = DateTime.Now;
            return await _context.MentalHealthFollowUps
                .Where(f => !f.IsCompleted && f.NextTipDate <= now)
                .ToListAsync();
        }

        public async Task<MentalHealthFollowUp> UpdateAsync(MentalHealthFollowUp followUp)
        {
            _context.MentalHealthFollowUps.Update(followUp);
            await _context.SaveChangesAsync();
            return followUp;
        }

        public async Task CompleteFollowUpAsync(int followUpId)
        {
            var followUp = await _context.MentalHealthFollowUps.FindAsync(followUpId);
            if (followUp != null)
            {
                followUp.IsCompleted = true;
                followUp.CompletedAt = DateTime.Now;
                await _context.SaveChangesAsync();
            }
        }
    }
}
