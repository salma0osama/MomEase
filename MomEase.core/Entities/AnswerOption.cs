using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MomEase.core.Entities
{
    public class AnswerOption
    {
        [Key]
        public int OptionId { get; set; }

        [Required]
        public int QuestionId { get; set; }

        [Required]
        public string OptionText { get; set; }
        public string? OptionTextAr { get; set; }
        [Required]
        public int Score { get; set; }

        public int? OptionOrder { get; set; }

        // Navigation Properties
        [ForeignKey("QuestionId")]
        public virtual Question Question { get; set; }

        public virtual ICollection<UserResponse> UserResponses { get; set; }
    }
}
