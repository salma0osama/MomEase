using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace PostCare.core.Entities
{
    public class Assessment
    {
        public int AssessmentId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int TotalQuestions { get; set; }
        public int MaxScore { get; set; }

        // Navigation Properties
        public virtual ICollection<ScoreLevel> ScoreLevels { get; set; }
        public virtual ICollection<Question> Questions { get; set; }
        public virtual ICollection<AssessmentResult> AssessmentResults { get; set; }
        public virtual ICollection<UserResponse> UserResponses { get; set; }

    }
}
