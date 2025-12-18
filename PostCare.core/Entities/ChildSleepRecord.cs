using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PostCare.core.Entities
{
    public class ChildSleepRecord
    {
        public int RecordId { get; set; }
        public int ChildId { get; set; }
        public DateTime SleepDate { get; set; }
        public TimeSpan? SleepHoursTotal { get; set; }
        public int? SleepRefId { get; set; }
        public string Notes { get; set; }

        // Navigation Properties
        public virtual Child Child { get; set; }
        public virtual SleepReference SleepReference { get; set; }
    }
}
