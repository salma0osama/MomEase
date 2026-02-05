using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MomEase.core.DTOS.GrowthTracking
{
    public class UpdateGrowthRecordDto
    {
        [Range(0.5, 100)]
        public decimal? WeightKg { get; set; }

        [Range(30, 200)]
        public decimal? HeightCm { get; set; }
    }
}
