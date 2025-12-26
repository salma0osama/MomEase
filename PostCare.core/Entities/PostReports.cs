using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PostCare.core.Entities
{
    public class PostReports
    {
        [Key]
        public int ReportId { get; set; }

        [Required]
        public int PostId { get; set; }

        [Required]
        public int ReporterId { get; set; }

        [Required]
        [MaxLength(500)]
        public string Reason { get; set; }

        public int? ReviewedById { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? ReviewedAt { get; set; }

        // Navigation Properties
        [ForeignKey("PostId")]
        public virtual CommunityPosts Post { get; set; }

        [ForeignKey("ReporterId")]
        public virtual Users Reporter { get; set; }

        [ForeignKey("ReviewedById")]
        public virtual Users ReviewedBy { get; set; }
    }
}