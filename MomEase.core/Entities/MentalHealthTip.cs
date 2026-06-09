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
        public string SeverityLevel { get; set; } // Severe, Moderate, Mild, Minimal

        [MaxLength(50)]
        public string? SeverityLevelAr { get; set; } // شديد، متوسط، خفيف، طبيعي

        [Required]
        public string TipTextEnglish { get; set; } // English (required)

        public string? TipTextAr { get; set; } // Arabic (optional)

        [MaxLength(100)]
        public string? Category { get; set; } // Self-care, Nutrition, Exercise, Sleep, Social, Professional

        [MaxLength(100)]
        public string? CategoryAr { get; set; } // العناية الذاتية، التغذية، التمارين، النوم، التواصل، مساعدة متخصصة

        [Required]
        public bool IsActive { get; set; } = true;

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.Now.AddHours(1);
    }
}
