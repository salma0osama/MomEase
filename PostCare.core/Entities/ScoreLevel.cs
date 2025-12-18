using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PostCare.core.Entities
{
    public class ScoreLevel
    {
        public int LevelId { get; set; }
        public int AssessmentId { get; set; }
        public string LevelName { get; set; }
        public int MinScore { get; set; }
        public int MaxScore { get; set; }
        public string Advice { get; set; }

        // Navigation Properties
        public virtual Assessment Assessment { get; set; }
        public virtual ICollection<AssessmentResult> AssessmentResults { get; set; }
    }

}
