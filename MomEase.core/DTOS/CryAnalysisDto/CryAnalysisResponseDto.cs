using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;


namespace MomEase.core.DTOS.CryAnalysisDto
{
    public class CryAnalysisResponseDto
    {
        public int CryId { get; set; }
        public string AudioUrl { get; set; }
        public string Result { get; set; }          // الاسم المعروض (عربي أو إنجليزي)
        public double Confidence { get; set; }
        public string Advice { get; set; }
        public int? ChildId { get; set; }
        public DateTime CreatedAt { get; set; }

        // All scores من الـ AI
        public Dictionary<string, double> AllScores { get; set; }
    }
}
