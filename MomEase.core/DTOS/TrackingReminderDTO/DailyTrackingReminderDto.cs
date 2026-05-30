using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MomEase.core.DTOS.TrackingReminderDTO
{
    public class DailyTrackingReminderDto
    {
        public int ReminderId { get; set; }
        public int UserId { get; set; }
        public DateTime ReminderDate { get; set; }
        public List<ChildTrackingStatusDto> ChildrenStatus { get; set; }
        public string Title { get; set; }  // مترجم
        public string Message { get; set; }  // مترجم
        public bool IsSent { get; set; }
        public string? ActionUrl { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
