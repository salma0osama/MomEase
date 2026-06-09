using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MomEase.core.Entities
{
    public class MentalHealthFollowUp
    {
        [Key]
        public int FollowUpId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public int LastAssessmentResultId { get; set; }

        [Required]
        [MaxLength(50)]
        public string SeverityLevel { get; set; } // Minimal, Mild, Moderate, Severe

        [Required]
        public DateTime NextAssessmentDate { get; set; }

        [Required]
        public DateTime NextTipDate { get; set; }

        [Required]
        public bool AssessmentReminderSent { get; set; } = false;

        [Required]
        public bool IsCompleted { get; set; } = false;

        public DateTime? CompletedAt { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.Now.AddHours(1);

        // Navigation Properties
        [ForeignKey("UserId")]
        public virtual Users User { get; set; }

        [ForeignKey("LastAssessmentResultId")]
        public virtual AssessmentResult LastAssessmentResult { get; set; }
    }
}
