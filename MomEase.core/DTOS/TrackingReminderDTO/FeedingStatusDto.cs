using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MomEase.core.DTOS.TrackingReminderDTO
{
    public class FeedingStatusDto
    {
        public int? LastRecordId { get; set; }
        public DateTime? LastRecordDate { get; set; }
        public int? TimesPerDay { get; set; }
        public string Status { get; set; }  // ✅ Updated / ⚠️ Outdated / ❌ Missing
        public string Message { get; set; }  // التنبيه (مترجم)
        public bool HasData { get; set; }
    }
}
