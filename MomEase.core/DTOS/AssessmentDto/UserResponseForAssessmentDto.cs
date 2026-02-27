using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MomEase.core.DTOS.AssessmentDto
{
    public class UserResponseForAssessmentDto
    {
        public int ResponseId { get; set; }
        public int QuestionId { get; set; }
        public string? QuestionText { get; set; }
        public int OptionId { get; set; }
        public string? OptionText { get; set; }
        public int Score { get; set; }
    }
}
