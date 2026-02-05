using MomEase.core.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MomEase.core.Entities
{
    public class FeedingReference
    {
        [Key]
        public int FeedingRefId { get; set; }

        [Required]
        public FeedingTypeForBaby FeedingTypeForBaby { get; set; }

        public int? AgeMinMonths { get; set; }

        public int? AgeMaxMonths { get; set; }

        public int? MinTimesPerDay { get; set; }

        public int? MaxTimesPerDay { get; set; }

        [MaxLength(500)]
        public string Notes { get; set; }

        // Navigation Property
        public virtual ICollection<ChildFeedingRecord> ChildFeedingRecords { get; set; }
    }
}
