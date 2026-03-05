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
    public class GrowthPercentileReferenceRepository : IGrowthPercentileReferenceRepository
    {
        private readonly MomEaseDbContext _context;

        public GrowthPercentileReferenceRepository(MomEaseDbContext context)
        {
            _context = context;
        }

        public async Task<GrowthPercentileReference> GetByGenderAgeAndMetricAsync(
            Gender gender,
            int ageMonths,
            MetricType metricType)
        {
            return await _context.GrowthPercentileReferences
                .FirstOrDefaultAsync(r =>
                    r.Gender == gender &&
                    r.AgeMonths == ageMonths &&
                    r.Metric_Type == metricType);
        }
    }
}
