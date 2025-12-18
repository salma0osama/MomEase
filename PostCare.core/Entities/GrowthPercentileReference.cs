using PostCare.core.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PostCare.core.Entities
{
    public class GrowthPercentileReference
    {
        public int RefId { get; set; }
        public Gender Gender { get; set; }
        public int AgeMonths { get; set; }
        public MetricType Metric_Type { get; set; }
        public decimal? P5 { get; set; }
        public decimal? P10 { get; set; }
        public decimal? P25 { get; set; }
        public decimal? P50 { get; set; }
        public decimal? P75 { get; set; }
        public decimal? P90 { get; set; }
        public decimal? P95 { get; set; }
        public string Notes { get; set; }
        //public virtual ICollection<GrowthRecords> GrowthRecords { get; set; }

    }

}
