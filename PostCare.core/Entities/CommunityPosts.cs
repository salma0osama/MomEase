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
        public int PostId { get; set; }
        public int UserId { get; set; }
        public MediaType? Mediatype { get; set; }
        public string Mediaurl { get; set; }
        public string Text { get; set; }
        public DateTime CreatedAt { get; set; }

        // Navigation Properties
        public virtual Users User { get; set; }
        public virtual ICollection<PostComments> PostComments { get; set; }
        public virtual ICollection<PostReactions> PostReactions { get; set; }
        public virtual ICollection<PostReports> PostReports { get; set; }
    }
}
