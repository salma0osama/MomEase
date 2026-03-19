using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MomEase.core.DTOS.AssessmentDto
{
    public class UpdateScoreLevelDto
    {
        [Range(0, int.MaxValue)]
        public int? MinScore { get; set; }

        [Range(0, int.MaxValue)]
        public int? MaxScore { get; set; }

        [MaxLength(100)]
        public string? LevelName { get; set; }

        public string? Advice { get; set; }
        public string? LevelNameAr { get; set; }
        public string? AdviceAr { get; set; }
    }
}
