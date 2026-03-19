using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MomEase.core.DTOS.AssessmentDto
{
    public class UpdateAssessmentDto
    {
        [MaxLength(255)]
        public string? Name { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }
        public string? NameAr { get; set; }
        public string? DescriptionAr { get; set; }
        [PositiveNumber]
        public int? TotalQuestions { get; set; }
        [PositiveNumber]
        public int? MaxScore { get; set; }
    }
}
