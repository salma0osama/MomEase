using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MomEase.core.DTOS.TrackingReminderDTO
{
    public class ChildTrackingStatusDto
    {
        public int ChildId { get; set; }
        public string ChildName { get; set; }
        public FeedingStatusDto FeedingStatus { get; set; }
        public SleepStatusDto SleepStatus { get; set; }
        public GrowthStatusDto GrowthStatus { get; set; }
        public int TotalMissingDataCount { get; set; }
    }
}
