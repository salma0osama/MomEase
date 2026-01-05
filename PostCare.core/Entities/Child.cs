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
    public class Child
    {
        [Key]
        public int ChildId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        [MaxLength(255)]
        public string FullName { get; set; }

        [Required]
        public Gender Gender { get; set; }

        [Required]
        public DateTime BirthDate { get; set; }

        [Required]
        public DeliveryType DeliveryType { get; set; }

        [Required]

        public FeedingTypeForBaby FeedingTypeForBaby { get; set; }

        [MaxLength(500)]
        public string? PhotoUrl { get; set; }

        // Navigation Properties
        [ForeignKey("UserId")]
        public virtual Users User { get; set; }

        public virtual ICollection<GrowthRecords> GrowthRecords { get; set; }
        public virtual ICollection<GrowthReports> GrowthReports { get; set; }
        public virtual ICollection<ChildSleepRecord> ChildSleepRecords { get; set; }
        public virtual ICollection<ChildFeedingRecord> ChildFeedingRecords { get; set; }
        public virtual ICollection<ChildVaccination> ChildVaccinations { get; set; }
        public virtual ICollection<SkinAnalyses> SkinAnalyses { get; set; }
        public virtual ICollection<CryAnalyses> CryAnalyses { get; set; }
    }

}
