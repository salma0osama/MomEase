using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MomEase.core.DTOS.GrowthReport
{
    public class ReportSummaryDto
    {
        public string OverallStatus { get; set; } // "ممتاز" / "جيد" / "يحتاج متابعة"
        public int TotalDays { get; set; }
        public int GrowthRecordsCount { get; set; }
        public int SleepRecordsCount { get; set; }
        public int FeedingRecordsCount { get; set; }
        public string KeyInsight { get; set; } // أهم ملاحظة في التقرير
    }
}
