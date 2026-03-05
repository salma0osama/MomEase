using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MomEase.core.DTOS.GrowthTracking
{
    public class WeeklyGrowthDto
    {
        public DateTime WeekStart { get; set; }
        public DateTime WeekEnd { get; set; }
        public List<DailyGrowthDto> DailyGrowth { get; set; }
        public decimal? WeeklyWeightGain { get; set; }
        public decimal? WeeklyHeightGain { get; set; }
        public int TotalRecords { get; set; }
    }
}
