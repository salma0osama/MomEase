using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MomEase.core.Entities
{
    public class ScoreLevel
    {
        [Key]
        public int LevelId { get; set; }

        [Required]
        public int AssessmentId { get; set; }

        [Required]
        [MaxLength(100)]
        public string LevelName { get; set; }

        [Required]
        public int MinScore { get; set; }

        [Required]
        public int MaxScore { get; set; }

        public string Advice { get; set; }

        // Navigation Properties
        [ForeignKey("AssessmentId")]
        public virtual Assessment Assessment { get; set; }

        public virtual ICollection<AssessmentResult> AssessmentResults { get; set; }
    }

}
