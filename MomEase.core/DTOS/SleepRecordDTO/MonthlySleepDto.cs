using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MomEase.core.DTOS.SleepRecordDTO
{
    public class MonthlySleepDto
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public string MonthName { get; set; }
        public List<DailySleepDto> DailySleep { get; set; }
        public TimeSpan MonthlyAverageSleep { get; set; }
        public string MonthlyAverageSleepFormatted { get; set; }
        public int TotalRecords { get; set; }
        public int GoodDays { get; set; }
        public int PoorDays { get; set; }
    }
}
