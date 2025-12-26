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
    public class MotherProfile
    {
        [Key]
        public int MotherId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public bool IsFirstTimeMother { get; set; } = false;

        [Required]
        public int NumberOfChildren { get; set; } = 0;

        public MentalHealthStatus? MentalHealthStatus { get; set; }

        public HealthStatus? HealthStatus { get; set; }

        // Navigation Property
        [ForeignKey("UserId")]
        public virtual Users User { get; set; }
    }
}
