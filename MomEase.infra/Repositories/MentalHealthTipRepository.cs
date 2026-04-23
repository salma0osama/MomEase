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
    public class MentalHealthTipRepository : IMentalHealthTipRepository
    {
        private readonly MomEaseDbContext _context;

        public MentalHealthTipRepository(MomEaseDbContext context)
        {
            _context = context;
        }

        public async Task<MentalHealthTip?> GetRandomTipAsync(int userId, string severityLevel)
        {
            var sentTipIds = await _context.SentMentalHealthTips
                .Where(st => st.UserId == userId)
                .Select(st => st.TipId)
                .ToListAsync();

            var availableTips = await _context.MentalHealthTips
                .Where(t => t.SeverityLevel == severityLevel
                         && t.IsActive
                         && !sentTipIds.Contains(t.TipId))
                .ToListAsync();

            // لو كل الـ Tips اتبعتت، reset ونبدأ من الأول
            if (!availableTips.Any())
            {
                var tipsToReset = await _context.SentMentalHealthTips
                    .Where(st => st.UserId == userId)
                    .ToListAsync();

                _context.SentMentalHealthTips.RemoveRange(tipsToReset);
                await _context.SaveChangesAsync();

                availableTips = await _context.MentalHealthTips
                    .Where(t => t.SeverityLevel == severityLevel && t.IsActive)
                    .ToListAsync();
            }

            if (!availableTips.Any()) return null;

            // اختار tip عشوائي
            var random = new Random();
            var randomIndex = random.Next(availableTips.Count);
            return availableTips[randomIndex];
        }

        public async Task MarkTipAsSentAsync(int userId, int tipId)
        {
            var sentTip = new SentMentalHealthTip
            {
                UserId = userId,
                TipId = tipId,
                SentAt = DateTime.Now
            };

            await _context.SentMentalHealthTips.AddAsync(sentTip);
            await _context.SaveChangesAsync();
        }
    }
}
