using MomEase.core.Entities;
using MomEase.core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MomEase.core.Interfaces
{
    public interface IGrowthPercentileReferenceRepository
    {
        Task<GrowthPercentileReference> GetByGenderAgeAndMetricAsync(
            Gender gender,
            int ageMonths,
            MetricType metricType);
    }
}
