using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MomEase.core.Entities
{
    public class PostReports
    {
        [Key]
        public int ReportId { get; set; }
        public int? PostId { get; set; }
        public int ReporterId { get; set; }
        [MaxLength(500)]
        public string Reason { get; set; } = string.Empty;
        public int? ReviewedById { get; set; }
        public string? Action { get; set; }
        public string? AdminNote { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now.AddHours(1);
        public DateTime? ReviewedAt { get; set; }

        [ForeignKey("PostId")]
        public virtual CommunityPosts Post { get; set; } = null!;
        [ForeignKey("ReporterId")]
        public virtual Users Reporter { get; set; } = null!;
        [ForeignKey("ReviewedById")]
        public virtual Users? ReviewedBy { get; set; }
    }
}