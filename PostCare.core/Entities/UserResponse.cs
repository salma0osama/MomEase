using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PostCare.core.Entities
{
    public class UserResponse
    {
        [Key]
        public int ResponseId { get; set; }

        [Required]
        public int ResultId { get; set; }

        [Required]
        public int QuestionId { get; set; }

        [Required]
        public int OptionId { get; set; }

        [Required]
        public int ComputedScore { get; set; }

        // Navigation Properties
        [ForeignKey("ResultId")]
        public virtual AssessmentResult AssessmentResult { get; set; }

        [ForeignKey("QuestionId")]
        public virtual Question Question { get; set; }

        [ForeignKey("OptionId")]
        public virtual AnswerOption AnswerOption { get; set; }
    }
}
