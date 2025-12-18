using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PostCare.core.Entities
{
    public class Diseases
    {
        public int DiseaseId { get; set; }
        public string Name { get; set; }
        public string Advice { get; set; }

        // Navigation Properties
        public virtual ICollection<SkinAnalyses> SkinAnalyses { get; set; }
    }
}
