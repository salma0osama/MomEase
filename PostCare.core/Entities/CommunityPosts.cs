using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PostCare.core.Enums;

namespace PostCare.core.Entities
{
    public class CommunityPosts
    {
        [Key]
        public int PostId { get; set; }

        [Required]
        public int UserId { get; set; }

        public MediaType? Mediatype { get; set; }

        [MaxLength(500)]
        public string Mediaurl { get; set; }

        public string Text { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation Properties
        [ForeignKey("UserId")]
        public virtual Users User { get; set; }

        public virtual ICollection<PostComments> PostComments { get; set; }
        public virtual ICollection<PostReactions> PostReactions { get; set; }
        public virtual ICollection<PostReports> PostReports { get; set; }
    }
}
