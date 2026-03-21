using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MomEase.core.DTOS
{
    public class UpdateLanguagePreferenceDto
    {
        [Required]
        public string Language { get; set; }
    }
}
