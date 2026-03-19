using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace MomEase.core.Entities
{
    public class Assessment
    {
        [Key]
        public int AssessmentId { get; set; }

        [Required]
        [MaxLength(255)]
        public string Name { get; set; }

        [MaxLength(1000)]
        public string Description { get; set; }
        public string? NameAr { get; set; }
        public string? DescriptionAr { get; set; }
        [Required]
        public int TotalQuestions { get; set; }

        [Required]
        public int MaxScore { get; set; }

        // Navigation Properties
        public virtual ICollection<ScoreLevel> ScoreLevels { get; set; }
        public virtual ICollection<Question> Questions { get; set; }
        public virtual ICollection<AssessmentResult> AssessmentResults { get; set; }
    }
}
