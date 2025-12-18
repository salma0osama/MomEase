using PostCare.core.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PostCare.core.Entities
{
    public class FeedingReference
    {
        public int FeedingRefId { get; set; }
        public FeedingType FeedingType { get; set; }
        public int? AgeMaxMonths { get; set; }
        public int? AgeMinMonths { get; set; }
        public int? MinTimesPerDay { get; set; }
        public int? MaxTimesPerDay { get; set; }

        public string Notes { get; set; }

        // Navigation Properties
        public virtual ICollection<ChildFeedingRecord> ChildFeedingRecords { get; set; }
        public virtual ICollection<GrowthReports> GrowthReports { get; set; }

    }
}
