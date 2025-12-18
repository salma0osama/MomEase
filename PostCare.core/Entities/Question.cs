using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PostCare.core.Entities
{
    public class Question
    {
        public int QuestionId { get; set; }
        public int AssessmentId { get; set; }
        public string QuestionText { get; set; }
        public int QuestionOrder { get; set; }
        public bool IsReverse { get; set; }

        // Navigation Properties
        public virtual Assessment Assessment { get; set; }
        public virtual ICollection<AnswerOption> AnswerOptions { get; set; }
        public virtual ICollection<UserResponse> UserResponses { get; set; }
    }
}
