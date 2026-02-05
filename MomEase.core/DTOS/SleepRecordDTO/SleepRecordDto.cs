using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MomEase.core.DTOS.SleepRecordDTO
{
    public class SleepRecordDto
    {
        public int RecordId { get; set; }
        public int ChildId { get; set; }
        public string ChildName { get; set; }
        public DateTime SleepDate { get; set; }
        public TimeSpan? SleepHoursTotal { get; set; }
        public string SleepHoursTotalFormatted { get; set; } // "8h 30m"
        public int? SleepRefId { get; set; }
        public string? Notes { get; set; }
        public string Status { get; set; } // "Good", "Normal", "Poor"
        public SleepingReferenceInfo? ReferenceInfo { get; set; }
    }
}
