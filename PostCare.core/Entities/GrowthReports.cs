using PostCare.core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PostCare.core.Entities
{
    public class GrowthReports
    {
        public int ReportId { get; set; }
        public int ChildId { get; set; }
        public int RefId { get; set; }
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public string ReportContent { get; set; }
        public GrowthStatus? GrowthStatus { get; set; }

        // Navigation Properties
        public virtual Child Child { get; set; }
        public virtual GrowthPercentileReference GrowthPercentileReference { get; set; }

    }
}
