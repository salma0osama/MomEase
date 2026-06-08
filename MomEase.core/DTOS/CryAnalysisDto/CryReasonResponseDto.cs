using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;


namespace MomEase.core.DTOS.CryAnalysisDto
{
    public class CryReasonResponseDto
    {
        public int CryreasonId { get; set; }
        public string Name { get; set; }
        public string Advice { get; set; }
    }
}
