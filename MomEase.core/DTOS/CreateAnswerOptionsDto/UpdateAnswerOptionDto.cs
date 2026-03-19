using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MomEase.core.DTOS.CreateAnswerOptionsDto
{
    public class UpdateAnswerOptionDto
    {
        public string? OptionText { get; set; }
        public string? OptionTextAr { get; set; }
        public int? Score { get; set; }
        public int? OptionOrder { get; set; }
    }
}
