using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PostCare.core.DTOS.SleepRecordDTO
{
    public class SleepStatisticsDto
    {
        // Basic Stats
        public int TotalRecords { get; set; }
        public TimeSpan AverageSleepHours { get; set; }
        public string AverageSleepHoursFormatted { get; set; }
        public TimeSpan MaxSleepHours { get; set; }
        public TimeSpan MinSleepHours { get; set; }

        // Quality Stats
        public int GoodSleepDays { get; set; }
        public int NormalSleepDays { get; set; }
        public int PoorSleepDays { get; set; }
        public double SleepQualityPercentage { get; set; }

        // ✅ New: Recent Performance
        public TimeSpan Last7DaysAverage { get; set; }
        public string Last7DaysAverageFormatted { get; set; }

        // ✅ New: Current Status
        public string CurrentSleepStatus { get; set; } // "Good", "Normal", "Poor"

        // ✅ New: Most Common Status
        public string MostCommonStatus { get; set; }

        // ✅ New: Comparison with Reference
        public ComparisonWithSleepReferenceDto ComparisonWithReference { get; set; }
    }
}
