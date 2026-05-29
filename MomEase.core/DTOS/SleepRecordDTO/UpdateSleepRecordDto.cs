using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MomEase.core.DTOS.SleepRecordDTO
{
    public class UpdateSleepRecordDto
    {
        public DateTime? SleepDate { get; set; }

        [RegularExpression(@"^([0-1][0-9]|2[0-3]):[0-5][0-9]$",
            ErrorMessage = "Start time must be in HH:mm format")]
        public string? SleepStartTime { get; set; }

        [RegularExpression(@"^([0-1][0-9]|2[0-3]):[0-5][0-9]$",
            ErrorMessage = "End time must be in HH:mm format")]
        public string? SleepEndTime { get; set; }

        // ✅ Remove old field
        // public string? SleepHoursTotal { get; set; }

        [MaxLength(50)]
        public string? Quality { get; set; }

        public int? SleepRefId { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }
    }
}
