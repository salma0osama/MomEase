using MomEase.core.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MomEase.core.Entities
{
    public class Diseases
    {
        [Key]
        public int DiseaseId { get; set; }

        [Required]
        [MaxLength(255)]
        public SkinAnalysisDiseaseName Name { get; set; }

        public string Advice { get; set; }

        // Navigation Property
        public virtual ICollection<SkinAnalyses> SkinAnalyses { get; set; }
    }
}
