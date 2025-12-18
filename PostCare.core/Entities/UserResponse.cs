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
        public int ResponseId { get; set; }
        public int UserId { get; set; }
        public int QuestionId { get; set; }
        public int OptionId { get; set; }
        public int ComputedScore { get; set; }
        public DateTime Answered_at { get; set; }


        // Navigation Properties
        public virtual Users Users { get; set; }
        public virtual Question Question { get; set; }
        public virtual AnswerOption AnswerOption { get; set; }
    }
}
