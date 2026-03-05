using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MomEase.core.DTOS.GrowthReport
{
    public class ChartDataPointDto
    {
        public DateTime Date { get; set; }
        public int AgeMonths { get; set; }
        public decimal Value { get; set; }
        public string Label { get; set; }
    }
}
