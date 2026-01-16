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
        [Key]
        public int RecordId { get; set; }

        [Required]
        public int ChildId { get; set; }

        [Required]
        public DateTime FeedingDate { get; set; }

        [Required]
        public int FeedingTimesPerDay { get; set; }

        [Required]
        public FeedingType FeedingType { get; set; }

        [Required]
        public FeedingTypeForBaby FeedingTypeForBaby { get; set; }

        public int? FeedingRefId { get; set; }

        [MaxLength(500)]
        public string Notes { get; set; }

        // Navigation Properties
        [ForeignKey("ChildId")]
        public virtual Child Child { get; set; }

        [ForeignKey("FeedingRefId")]
        public virtual FeedingReference FeedingReference { get; set; }
    }
}
