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
    public class GrowthReports
    {
        [Key]
        public int ReportId { get; set; }

        [Required]
        public int ChildId { get; set; }

        public int? RefId { get; set; }

        [Required]
        public DateTime PeriodStart { get; set; }

        [Required]
        public DateTime PeriodEnd { get; set; }


        public string ReportContent { get; set; }

        public string? GrowthStatus { get; set; }

        // Navigation Properties
        [ForeignKey("ChildId")]
        public virtual Child Child { get; set; }

        [ForeignKey("RefId")]
        public virtual GrowthPercentileReference GrowthPercentileReference { get; set; }
    }
}
