using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PostCare.core.Entities
{
    public class CryReasons
    {
        [Key]
        public int CryreasonId { get; set; }

        [Required]
        [MaxLength(255)]
        public string Name { get; set; }

        public string Advice { get; set; }

        // Navigation Property
        public virtual ICollection<CryAnalyses> CryAnalyses { get; set; }
    }
}
