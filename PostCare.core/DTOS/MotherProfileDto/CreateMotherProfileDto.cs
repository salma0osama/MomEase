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
    /// DTO for creating mother profile (automatically created during registration)
    /// </summary>
    public class CreateMotherProfileDto
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        public bool IsFirstTimeMother { get; set; } = true;

        [Required]
        [Range(0, 20)]
        public int NumberOfChildren { get; set; } = 0;

        public MentalHealthStatus? MentalHealthStatus { get; set; }

        public HealthStatus? HealthStatus { get; set; }
    }
}
