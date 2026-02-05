using MomEase.core.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MomEase.core.Entities
{
    public class GrowthPercentileReference
    {
        [Key]
        public int RefId { get; set; }

        [Required]
        public Gender Gender { get; set; }

        [Required]
        public int AgeMonths { get; set; }

        [Required]
        public MetricType Metric_Type { get; set; }

        public decimal? P5 { get; set; }
        public decimal? P10 { get; set; }
        public decimal? P25 { get; set; }
        public decimal? P50 { get; set; }
        public decimal? P75 { get; set; }
        public decimal? P90 { get; set; }
        public decimal? P95 { get; set; }

        [MaxLength(500)]
        public string Notes { get; set; }

        // Navigation Property
        public virtual ICollection<GrowthReports> GrowthReports { get; set; }
    }

}
