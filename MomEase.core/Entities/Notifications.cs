using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MomEase.core.Entities
{
    public class Notifications
    {
        [Key]
        public int NotificationId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        [MaxLength(255)]
        public string Title { get; set; }

        [Required]
        public string Body { get; set; }

        [MaxLength(50)]
        public string Type { get; set; } // ⬅️ إضافة النوع (VaccinationUpcoming, AssessmentResult, etc.)

        public int? RelatedEntityId { get; set; } // ⬅️ إضافة (مثلاً: PostId, VaccinationId)

        [MaxLength(500)]
        public string? ActionUrl { get; set; } // ⬅️ nullable

        [Required]
        public bool IsRead { get; set; } = false;

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? ReadAt { get; set; } // ⬅️ إضافة

        // Navigation Property
        [ForeignKey("UserId")]
        public virtual Users User { get; set; }
    }
}
