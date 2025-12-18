using PostCare.core.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PostCare.core.Entities
{
    public class ChildFeedingRecord
    {
        public int RecordId { get; set; }
        public int ChildId { get; set; }
        public DateTime FeedingDate { get; set; }
        public FeedingType FeedingType { get; set; }
        public int? NumberOfCalories { get; set; }
        public int? NumberOfMeals { get; set; }
        public int? FeedingRefId { get; set; }
        public string Notes { get; set; }
        public int? MaxTimesPerDay { get; set; }

        // Navigation Properties
        public virtual Child Child { get; set; }
        public virtual FeedingReference FeedingReference { get; set; }
    }
}
