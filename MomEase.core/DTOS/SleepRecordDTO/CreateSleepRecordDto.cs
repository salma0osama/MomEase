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

        public string? SleepHoursTotal { get; set; }


        [MaxLength(500, ErrorMessage = "الملاحظات يجب أن تكون أقل من 500 حرف")]
        public string? Notes { get; set; }
    }
}
