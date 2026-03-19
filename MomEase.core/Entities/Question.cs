using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MomEase.core.Entities
{
    public class Question
    {
        [Key]
        public int QuestionId { get; set; }

        [Required]
        public int AssessmentId { get; set; }

        [Required]
        public string QuestionText { get; set; }
        public string? QuestionTextAr { get; set; }
        [Required]
        public int QuestionOrder { get; set; }

        [Required]
        public bool IsReverse { get; set; } = false;

        // Navigation Properties
        [ForeignKey("AssessmentId")]
        public virtual Assessment Assessment { get; set; }

        public virtual ICollection<AnswerOption> AnswerOptions { get; set; }
        public virtual ICollection<UserResponse> UserResponses { get; set; }
    }
}
