using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PostCare.core.DTOS.SleepRecordDTO
{
    public class SleepStatisticsDto
    {
        public int TotalRecords { get; set; }
        public TimeSpan AverageSleepHours { get; set; }
        public string AverageSleepHoursFormatted { get; set; }
        public TimeSpan MaxSleepHours { get; set; }
        public TimeSpan MinSleepHours { get; set; }
        public int GoodSleepDays { get; set; }
        public int PoorSleepDays { get; set; }
        public double SleepQualityPercentage { get; set; }
    }
}
