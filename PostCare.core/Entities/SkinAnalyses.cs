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
        public int SkinanalysisId { get; set; }
        public int UserId { get; set; }
        public int? ChildId { get; set; }
        public string ImageUrl { get; set; }
        public string Result { get; set; }
        public int? DiseaseId { get; set; }
        public DateTime CreatedAt { get; set; }

        // Navigation Properties
        public virtual Users User { get; set; }
        public virtual Child Child { get; set; }
        public virtual Diseases Disease { get; set; }
    }
}
