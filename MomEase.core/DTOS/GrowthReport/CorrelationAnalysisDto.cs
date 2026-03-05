using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MomEase.core.DTOS.GrowthReport
{
    public class CorrelationAnalysisDto
    {
        public SleepGrowthCorrelationDto SleepAndGrowth { get; set; }
        public FeedingGrowthCorrelationDto FeedingAndGrowth { get; set; }
        public string OverallInsight { get; set; }
    }
}
