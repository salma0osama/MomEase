using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MomEase.core.DTOS.GrowthTracking
{
    public class MonthlyGrowthDto
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public string MonthName { get; set; }
        public List<DailyGrowthDto> DailyGrowth { get; set; }
        public decimal? MonthlyWeightGain { get; set; }
        public decimal? MonthlyHeightGain { get; set; }
        public int TotalRecords { get; set; }
        public int GoodGrowthDays { get; set; }
        public int PoorGrowthDays { get; set; }
    }
}
