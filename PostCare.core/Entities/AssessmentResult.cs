using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace PostCare.core.Entities
{
    public class AssessmentResult
    {
        public int ResultId { get; set; }
        public int UserId { get; set; }
        public int AssessmentId { get; set; }
        public int TotalScore { get; set; }
        public int? LevelId { get; set; }
        public DateTime CompletedAt { get; set; }

        // Navigation Properties
        public virtual Users User { get; set; }
        public virtual Assessment Assessment { get; set; }
        public virtual ScoreLevel ScoreLevel { get; set; }
    }
}
