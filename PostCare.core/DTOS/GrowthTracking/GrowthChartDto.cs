using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PostCare.core.DTOS.GrowthTracking
{
    public class GrowthChartDto
    {
        public string ChildName { get; set; }
        public List<ChartPoint> WeightData { get; set; }
        public List<ChartPoint> HeightData { get; set; }
    }

    public class ChartPoint
    {
        public DateTime Date { get; set; }
        public int AgeInWeeks { get; set; }
        public decimal Value { get; set; }
    }
}
