using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MomEase.core.DTOS.AssessmentDto
{
    public class AnswerDto
    {
        [Required]
        public int QuestionId { get; set; }

        [Required]
        public int OptionId { get; set; }
    }
}
