using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MomEase.core.DTOS.TrackingReminderDTO
{
    public class GrowthStatusDto
    {
        public int? LastRecordId { get; set; }
        public DateTime? LastRecordDate { get; set; }
        public double? Weight { get; set; }
        public double? Height { get; set; }
        public string Status { get; set; }
        public string Message { get; set; }
        public int DaysSinceLastRecord { get; set; }
        public bool HasData { get; set; }
    }
}
