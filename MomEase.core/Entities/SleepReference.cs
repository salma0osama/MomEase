using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MomEase.core.Entities
{
    public class SleepReference
    {
        [Key]
        public int SleepRefId { get; set; }

        public int? AgeMinMonths { get; set; }

        public int? AgeMaxMonths { get; set; }

        public TimeSpan? SleepMinHours { get; set; }

        public TimeSpan? SleepMaxHours { get; set; }

        [MaxLength(500)]
        public string Notes { get; set; }

        // Navigation Property
        public virtual ICollection<ChildSleepRecord> ChildSleepRecords { get; set; }
    }
}
