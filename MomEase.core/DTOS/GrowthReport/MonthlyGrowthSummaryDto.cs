using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MomEase.core.DTOS.GrowthReport
{
    public class MonthlyGrowthSummaryDto
    {
        public string Month { get; set; } // "January 2024"
        public decimal WeightGain { get; set; }
        public decimal HeightGain { get; set; }
        public string Status { get; set; } // Good/Poor
    }

}
