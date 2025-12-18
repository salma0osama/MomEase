using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PostCare.core.Entities
{
    public class PostComments
    {
        public int CommentId { get; set; }
        public int PostId { get; set; }
        public int UserId { get; set; }
        public string Text { get; set; }

        // Navigation Properties
        public virtual CommunityPosts Post { get; set; }
        public virtual Users User { get; set; }
    }

}
