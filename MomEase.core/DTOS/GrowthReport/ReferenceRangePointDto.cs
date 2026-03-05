using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MomEase.core.DTOS.GrowthReport
{
    public class ReferenceRangePointDto
    {
        public DateTime Date { get; set; }
        public int AgeMonths { get; set; }
        public decimal Min { get; set; }
        public decimal Max { get; set; }
    }
}
