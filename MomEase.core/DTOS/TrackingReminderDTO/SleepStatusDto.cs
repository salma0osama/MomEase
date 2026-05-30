using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MomEase.core.DTOS.TrackingReminderDTO
{
    public class SleepStatusDto
    {
        public int? LastRecordId { get; set; }
        public DateTime? LastRecordDate { get; set; }
        public string DurationFormatted { get; set; }  // مثل "10h 30m"
        public string Status { get; set; }
        public string Message { get; set; }
        public bool HasData { get; set; }
    }
}
