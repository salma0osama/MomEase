using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MomEase.core.Entities
{
    public class CryAnalyses
    {
        [Key]
        public int CryId { get; set; }

        [Required]
        public int UserId { get; set; }

        public int? ChildId { get; set; }

        [Required]
        [MaxLength(500)]
        public string AudioUrl { get; set; }

        public string Result { get; set; }

        public double Confidence { get; set; }

        public int? CryreasonId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now.AddHours(1);

        // Navigation Properties
        [ForeignKey("UserId")]
        public virtual Users User { get; set; }

        [ForeignKey("ChildId")]
        public virtual Child Child { get; set; }

        [ForeignKey("CryreasonId")]
        public virtual CryReasons CryReason { get; set; }
    }
}