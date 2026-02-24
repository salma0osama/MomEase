using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MomEase.core.DTOS.SkinAnalysisDto
{
    public class SkinAnalysisRequestDto
    {
        [Required]
        public int UserId { get; set; }

        public int? ChildId { get; set; }
    }
}
