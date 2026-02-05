using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MomEase.core.DTOS.GrowthTracking
{
    public class CreateGrowthRecordDto
    {
        [Required]
        [Range(0.5, 100)]
        public decimal WeightKg { get; set; }

        [Required]
        [Range(30, 200)]
        public decimal HeightCm { get; set; }
    }
}
