using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PostCare.core.Entities
{
    public class CryAnalyses
    {
        [Key]
        public int CryId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public int ChildId { get; set; }

        [Required]
        [MaxLength(500)]
        public string AudioUrl { get; set; }

        public string Result { get; set; }

        public int? CryreasonId { get; set; }

        // Navigation Properties
        [ForeignKey("UserId")]
        public virtual Users User { get; set; }

        [ForeignKey("ChildId")]
        public virtual Child Child { get; set; }

        [ForeignKey("CryreasonId")]
        public virtual CryReasons CryReason { get; set; }
    }
}
