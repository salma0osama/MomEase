using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PostCare.core.Entities
{
    public class GrowthRecords
    {
        [Key]
        public int GrowthId { get; set; }

        [Required]
        public int ChildId { get; set; }

        [Required]
        public DateTime RecordDate { get; set; }

        [Required]
        public int AgeInWeeks { get; set; }

        public decimal? WeightKg { get; set; }

        public decimal? HeightCm { get; set; }

        // Navigation Property
        [ForeignKey("ChildId")]
        public virtual Child Child { get; set; }
    }
}
