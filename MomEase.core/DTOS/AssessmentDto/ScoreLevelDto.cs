using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MomEase.core.DTOS.AssessmentDto
{
    public class ScoreLevelDto
    {
        public int LevelId { get; set; }
        public int AssessmentId { get; set; }
        public int MinScore { get; set; }
        public int MaxScore { get; set; }
        public string LevelName { get; set; }
        public string? Advice { get; set; }
    }
}
