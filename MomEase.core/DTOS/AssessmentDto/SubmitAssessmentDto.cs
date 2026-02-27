using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MomEase.core.DTOS.AssessmentDto
{
    public class SubmitAssessmentDto
    {
        [Required]
        public List<AnswerDto> Answers { get; set; }
    }
}
