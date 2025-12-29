using PostCare.core.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PostCare.core.DTOS.MotherProfileDto
{
    /// <summary>
    /// DTO for updating mother profile by the mother herself
    /// </summary>
    public class UpdateMotherProfileDto
    {
        [Required]
        public bool IsFirstTimeMother { get; set; }

        [Required]
        [Range(0, 20)]
        public int NumberOfChildren { get; set; }

        public MentalHealthStatus? MentalHealthStatus { get; set; }

        public HealthStatus? HealthStatus { get; set; }
    }
}
