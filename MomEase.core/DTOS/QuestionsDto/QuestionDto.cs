using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MomEase.core.DTOS.QuestionsDto
{
    public class QuestionDto
    {
        public int QuestionId { get; set; }
        public int AssessmentId { get; set; }
        public string QuestionText { get; set; }
        public string? QuestionTextAr { get; set; }

        public int QuestionOrder { get; set; }
        public bool IsReverse { get; set; }
    }
}
