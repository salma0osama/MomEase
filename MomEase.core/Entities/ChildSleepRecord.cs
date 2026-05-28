using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MomEase.core.Entities
{
    public class ChildSleepRecord
    {
        [Key]
        public int RecordId { get; set; }

        [Required]
        public int ChildId { get; set; }

        [Required]
        public DateTime SleepDate { get; set; }

        [Required]
        public TimeSpan SleepStartTime { get; set; }

        [Required]
        public TimeSpan SleepEndTime { get; set; }

        [MaxLength(50)]
        public string? Quality { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }

        public int? SleepRefId { get; set; }

        [ForeignKey("ChildId")]
        public virtual Child Child { get; set; }

        [ForeignKey("SleepRefId")]
        public virtual SleepReference SleepReference { get; set; }

        [NotMapped]
        public TimeSpan SleepDuration
        {
            get
            {
                if (SleepEndTime < SleepStartTime)
                {
                    // Overnight sleep
                    return (TimeSpan.FromHours(24) - SleepStartTime) + SleepEndTime;
                }
                else
                {
                    return SleepEndTime - SleepStartTime;
                }
            }
        }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
