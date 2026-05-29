using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MomEase.core.DTOS.AssessmentDto
{
    public class AssessmentResultDetailsDto
    {
        public int ResultId { get; set; }
        public int AssessmentId { get; set; }
        public int TotalScore { get; set; }
        public string? LevelName { get; set; }
        public string? Advice { get; set; }
        public List<string>? Recommendations { get; set; } // ⬅️ List<string>

        public DateTime CompletedAt { get; set; }
        public List<UserResponseForAssessmentDto>? Responses { get; set; }
    }
}
