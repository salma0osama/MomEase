using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MomEase.core.DTOS.GrowthTracking
{
    public class GrowthStatisticsDto
    {
        public int TotalRecords { get; set; }

        // Weight stats
        public decimal AverageWeight { get; set; }
        public decimal MaxWeight { get; set; }
        public decimal MinWeight { get; set; }
        public decimal WeightGainTotal { get; set; }
        public decimal MonthlyWeightGainAverage { get; set; }

        // Height stats
        public decimal AverageHeight { get; set; }
        public decimal MaxHeight { get; set; }
        public decimal MinHeight { get; set; }
        public decimal HeightGainTotal { get; set; }
        public decimal MonthlyHeightGainAverage { get; set; }

        // Status
        public string CurrentGrowthStatus { get; set; } // Normal/Underweight/Overweight
        public string WeightTrend { get; set; } // Increasing/Stable/Decreasing
        public string HeightTrend { get; set; }

        // Percentile info (optional for now)
        public ComparisonWithPercentileDto? PercentileComparison { get; set; }
    }
}
