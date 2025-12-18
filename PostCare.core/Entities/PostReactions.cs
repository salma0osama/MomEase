using PostCare.core.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PostCare.core.Entities
{
    public class PostReactions
    {
        public int ReactionId { get; set; }
        public int PostId { get; set; }
        public int UserId { get; set; }
        public ReactionType ReactionType { get; set; }

        // Navigation Properties
        public virtual CommunityPosts Post { get; set; }
        public virtual Users User { get; set; }
    }
}
