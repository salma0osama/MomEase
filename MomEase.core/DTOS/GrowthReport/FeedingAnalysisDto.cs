using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MomEase.core.DTOS.GrowthReport
{
    public class FeedingAnalysisDto
    {
        public double AverageFeedingsPerDay { get; set; }
        public int GoodFeedingDays { get; set; }
        public int PoorFeedingDays { get; set; }
        public string CurrentStatus { get; set; }
        public string Message { get; set; }
    }
}
