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

        public string? SleepHoursTotal { get; set; }

        public int? SleepRefId { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }
    }
}
