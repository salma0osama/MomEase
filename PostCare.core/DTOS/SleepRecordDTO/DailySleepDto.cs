using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PostCare.core.DTOS.SleepRecordDTO
{
    public class DailySleepDto
    {
        public DateTime Date { get; set; }
        public TimeSpan? SleepHours { get; set; }
        public string Status { get; set; }
    }
}
