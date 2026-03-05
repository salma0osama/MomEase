using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MomEase.core.DTOS.GrowthReport
{
    public class ReportChartsDto
    {
        public List<ChartDataPointDto> WeightChart { get; set; }
        public List<ChartDataPointDto> HeightChart { get; set; }
        public List<ChartDataPointDto> SleepChart { get; set; }
        public List<ChartDataPointDto> FeedingChart { get; set; }
        public List<PercentileLineDto> WeightPercentiles { get; set; }
        public List<PercentileLineDto> HeightPercentiles { get; set; }
        public List<ReferenceRangePointDto> SleepReference { get; set; }
        public List<ReferenceRangePointDto> FeedingReference { get; set; }
    }
}
