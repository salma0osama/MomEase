using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PostCare.core.Entities
{
    public class AnswerOption
    {
        public int OptionId { get; set; }
        public int QuestionId { get; set; }
        public string OptionText { get; set; }
        public int Score { get; set; }
        public int? OptionOrder { get; set; }

        // Navigation Properties
        public virtual Question Question { get; set; }
        public virtual ICollection<UserResponse> UserResponses { get; set; }
    }
}
