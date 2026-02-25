using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MomEase.core.DTOS.CreateAnswerOptionsDto
{
    public class AnswerOptionDto
    {
        public int OptionId { get; set; }
        public int QuestionId { get; set; }
        public string OptionText { get; set; }
        public int Score { get; set; }
        public int? OptionOrder { get; set; }
    }
}
