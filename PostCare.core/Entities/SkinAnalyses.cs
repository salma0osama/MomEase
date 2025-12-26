using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PostCare.core.Entities
{
    public class SkinAnalyses
    {
        [Key]
        public int SkinanalysisId { get; set; }

        [Required]
        public int UserId { get; set; }

        public int? ChildId { get; set; }

        [Required]
        [MaxLength(500)]
        public string ImageUrl { get; set; }

        public string Result { get; set; }

        public int? DiseaseId { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation Properties
        [ForeignKey("UserId")]
        public virtual Users User { get; set; }

        [ForeignKey("ChildId")]
        public virtual Child Child { get; set; }

        [ForeignKey("DiseaseId")]
        public virtual Diseases Disease { get; set; }
    }
}
