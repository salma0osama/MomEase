using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MomEase.core.DTOS.CryAnalysisDto
{
    public class CryReasonUpdateDto
    {
        [MaxLength(255)]
        public string Name { get; set; }

        [MaxLength(255)]
        public string NameAr { get; set; }

        public string Advice { get; set; }

        public string AdviceAr { get; set; }
    }
}
