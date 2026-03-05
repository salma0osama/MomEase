using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MomEase.core.DTOS.GrowthReport
{
    public class PercentileLineDto
    {
        public DateTime Date { get; set; }
        public int AgeMonths { get; set; }
        public decimal P5 { get; set; }
        public decimal P95 { get; set; }

    }
}
