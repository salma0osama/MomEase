using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MomEase.core.DTOS.GrowthTracking
{
    public class DailyGrowthDto
    {
        public DateTime Date { get; set; }
        public decimal? Weight { get; set; }
        public decimal? Height { get; set; }
        public string Status { get; set; } // Normal/Underweight/Overweight
    }
}
