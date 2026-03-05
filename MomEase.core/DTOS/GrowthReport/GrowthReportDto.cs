using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MomEase.core.DTOS.GrowthReport
{
    public class GrowthReportDto
    {
        public int ReportId { get; set; }
        public int ChildId { get; set; }
        public string ChildName { get; set; }
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public string GrowthStatus { get; set; } // Normal/Underweight/Overweight
        public DateTime CreatedAt { get; set; }

        // ✅ التقرير الكامل
        public GrowthReportContentDto ReportContent { get; set; }
    }
}
