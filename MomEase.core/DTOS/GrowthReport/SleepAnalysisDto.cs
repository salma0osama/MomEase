using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MomEase.core.DTOS.GrowthReport
{
    public class SleepAnalysisDto
    {
        public double AverageSleepHours { get; set; }
        public int GoodSleepDays { get; set; }
        public int PoorSleepDays { get; set; }
        public string CurrentStatus { get; set; }
        public string Message { get; set; }
    }
}
