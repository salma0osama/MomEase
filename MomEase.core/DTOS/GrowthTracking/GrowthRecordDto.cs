using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MomEase.core.DTOS.GrowthTracking
{
    public class GrowthRecordDto
    {
        public int GrowthId { get; set; }
        public string ChildName { get; set; }
        public DateTime RecordDate { get; set; }
        public int AgeInWeeks { get; set; }
        public int AgeInMonths => AgeInWeeks / 4;
        public decimal WeightKg { get; set; }
        public decimal HeightCm { get; set; }
    }
}
