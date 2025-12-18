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
        public int GrowthId { get; set; }
        public int ChildId { get; set; }
        public DateTime RecordDate { get; set; }
        public int AgeInWeeks { get; set; }
        public decimal? WeightKg { get; set; }
        public decimal? HeightCm { get; set; }

        // Navigation Properties
        public virtual Child Child { get; set; }
    }
}
