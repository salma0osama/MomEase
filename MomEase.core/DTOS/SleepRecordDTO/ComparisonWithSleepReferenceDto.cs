using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MomEase.core.DTOS.SleepRecordDTO
{

    /// <summary>
    /// DTO for comparing actual sleep with reference values
    /// </summary>
    public class ComparisonWithSleepReferenceDto
    {
        public string Status { get; set; }
        public double RecommendedMinHours { get; set; }
        public double RecommendedMaxHours { get; set; }
        public double ActualAverageHours { get; set; }
        public string Message { get; set; }
    }
}
