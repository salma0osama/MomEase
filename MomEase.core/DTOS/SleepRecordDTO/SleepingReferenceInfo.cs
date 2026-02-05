using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MomEase.core.DTOS.SleepRecordDTO
{
    /// <summary>
    /// Reference information included in responses
    /// </summary>
    public class SleepingReferenceInfo
    {
        public TimeSpan SleepMinHours { get; set; }
        public TimeSpan SleepMaxHours { get; set; }
        public string SleepMinHoursFormatted { get; set; }
        public string SleepMaxHoursFormatted { get; set; }
        public string AgeRange { get; set; }
    }
}
