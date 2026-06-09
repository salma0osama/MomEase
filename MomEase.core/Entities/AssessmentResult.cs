using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace MomEase.core.Entities
{
    public class AssessmentResult
    {
        [Key]
        public int ResultId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public int AssessmentId { get; set; }

        [Required]
        public int TotalScore { get; set; }

        public int? LevelId { get; set; }

        [Required]
        public DateTime CompletedAt { get; set; } = DateTime.Now.AddHours(1);

        // Navigation Properties
        [ForeignKey("UserId")]
        public virtual Users User { get; set; }

        [ForeignKey("AssessmentId")]
        public virtual Assessment Assessment { get; set; }

        [ForeignKey("LevelId")]
        public virtual ScoreLevel ScoreLevel { get; set; }

        public virtual ICollection<UserResponse> UserResponses { get; set; }
    }
}
