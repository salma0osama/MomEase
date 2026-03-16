using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MomEase.core.Entities
{
    public class MentalHealthTip
    {
        [Key]
        public int TipId { get; set; }

        [Required]
        [MaxLength(50)]
        public string SeverityLevel { get; set; } // Minimal, Mild, Moderate, Severe

        [Required]
        public string TipTextArabic { get; set; }

        public string? TipTextEnglish { get; set; }

        [MaxLength(100)]
        public string? Category { get; set; } // Self-care, Nutrition, Exercise, Sleep, Social, Professional

        [Required]
        public bool IsActive { get; set; } = true;

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
