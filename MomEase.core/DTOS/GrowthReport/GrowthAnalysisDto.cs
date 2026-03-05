using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MomEase.core.DTOS.GrowthReport
{
    public class GrowthAnalysisDto
    {
        // معدلات الزيادة
        public decimal TotalWeightGain { get; set; }
        public decimal TotalHeightGain { get; set; }
        public decimal MonthlyWeightGainAverage { get; set; }
        public decimal MonthlyHeightGainAverage { get; set; }

        // المقارنة بالطبيعي
        public string WeightStatus { get; set; } // Normal/Underweight/Overweight
        public string HeightStatus { get; set; }

        // الاتجاه
        public string Trend { get; set; } // Increasing/Stable/Decreasing

        // أي شهور كان فيها مشكلة
        public List<MonthlyGrowthSummaryDto> MonthlyBreakdown { get; set; }
    }
}
