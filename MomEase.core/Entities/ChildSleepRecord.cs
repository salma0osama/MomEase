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

        public TimeSpan? SleepHoursTotal { get; set; }

        public int? SleepRefId { get; set; }

        [MaxLength(500)]
        public string Notes { get; set; }

        // Navigation Properties
        [ForeignKey("ChildId")]
        public virtual Child Child { get; set; }

        [ForeignKey("SleepRefId")]
        public virtual SleepReference SleepReference { get; set; }
    }
}
