using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MomEase.core.DTOS.SleepRecordDTO
{
    public class WeeklySleepDto
    {
        public DateTime WeekStart { get; set; }
        public DateTime WeekEnd { get; set; }
        public List<DailySleepDto> DailySleep { get; set; }
        public TimeSpan WeeklyAverageSleep { get; set; }
        public int TotalRecords { get; set; }
    }
}
