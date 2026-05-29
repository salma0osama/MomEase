using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MomEase.core.DTOS.SleepRecordDTO
{
    public class CreateSleepRecordDto
    {
        [Required(ErrorMessage = "معرف الطفل مطلوب")]
        public int ChildId { get; set; }

        [Required(ErrorMessage = "تاريخ النوم مطلوب")]
        public DateTime SleepDate { get; set; }

        [Required]
        [RegularExpression(@"^([0-1][0-9]|2[0-3]):[0-5][0-9]$",
            ErrorMessage = "Start time must be in HH:mm format (e.g., 20:30)")]
        public string SleepStartTime { get; set; }  // "20:30"

        [Required]
        [RegularExpression(@"^([0-1][0-9]|2[0-3]):[0-5][0-9]$",
            ErrorMessage = "End time must be in HH:mm format (e.g., 07:00)")]
        public string SleepEndTime { get; set; }    // "07:00"
        [MaxLength(50)]
        public string? Quality { get; set; }  // Good, Fair, Poor

        [MaxLength(500, ErrorMessage = "الملاحظات يجب أن تكون أقل من 500 حرف")]
        public string? Notes { get; set; }
    }
}
