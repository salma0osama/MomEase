using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PostCare.core.Entities
{
    public class SleepReference
    {
        public int SleepRefId { get; set; }
        public int? AgeMaxMonths { get; set; }
        public int? AgeMinMonths { get; set; }
        public TimeSpan? SleepMinHours { get; set; }
        public TimeSpan? SleepMaxHours { get; set; }
        public string Notes { get; set; }

        // Navigation Properties
        public virtual ICollection<ChildSleepRecord> ChildSleepRecords { get; set; }
        public virtual ICollection<GrowthReports> GrowthReports { get; set; }

    }
}
