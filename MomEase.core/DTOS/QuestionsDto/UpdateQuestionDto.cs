using MomEase.core.DTOS.AssessmentDto;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MomEase.core.DTOS.QuestionsDto
{
    public class UpdateQuestionDto
    {
        public string? QuestionText { get; set; }

        [PositiveNumber]
        public int? QuestionOrder { get; set; }

        public bool? IsReverse { get; set; }
    }
}
