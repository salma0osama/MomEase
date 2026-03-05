using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MomEase.core.DTOS.GrowthTracking
{
    public class ComparisonWithPercentileDto
    {
        public string Status { get; set; }
        public decimal CurrentWeightPercentile { get; set; }
        public decimal CurrentHeightPercentile { get; set; }
        public string Message { get; set; }
    }
}
