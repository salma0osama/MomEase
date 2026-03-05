using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MomEase.core.DTOS.GrowthReport
{
    public class FeedingGrowthCorrelationDto
    {
        public bool HasCorrelation { get; set; }
        public string Type { get; set; }
        public string Message { get; set; }
    }
}
