using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MomEase.core.DTOS.SkinAnalysisDto
{
    public class SkinAnalysisResponseDto
    {
        public int SkinanalysisId { get; set; }
        public string ImageUrl { get; set; }
        public string Result { get; set; }
        public string DiseaseName { get; set; }
        public string Advice { get; set; }
        public double? Confidence { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
