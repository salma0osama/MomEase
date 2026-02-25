using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MomEase.core.DTOS.CreateAnswerOptionsDto
{
    public class CreateAnswerOptionDto
    {
        [Required]
        public string OptionText { get; set; }

        [Required]
        public int Score { get; set; }

        public int? OptionOrder { get; set; }
    }
}
