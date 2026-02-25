using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MomEase.core.DTOS.AssessmentDto
{
    public class AssessmentDto
    {
        public int AssessmentId { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public int TotalQuestions { get; set; }
        public int MaxScore { get; set; }
    }
}
