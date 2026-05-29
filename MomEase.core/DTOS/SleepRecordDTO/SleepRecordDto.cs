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
        public TimeSpan SleepStartTime { get; set; }
        public TimeSpan SleepEndTime { get; set; }
        public string SleepStartTimeFormatted { get; set; }  // "20:30"
        public string SleepEndTimeFormatted { get; set; }
        public TimeSpan SleepDuration { get; set; }
        public string SleepDurationFormatted { get; set; }
        public string? Quality { get; set; }
        public int? SleepRefId { get; set; }
        public string? Notes { get; set; }
        public string Status { get; set; } // "Good", "Normal", "Poor"
        public SleepingReferenceInfo? ReferenceInfo { get; set; }
    }
}
