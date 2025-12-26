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
        [Key]
        public int CommentId { get; set; }

        [Required]
        public int PostId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public string Text { get; set; }

        // Navigation Properties
        [ForeignKey("PostId")]
        public virtual CommunityPosts Post { get; set; }

        [ForeignKey("UserId")]
        public virtual Users User { get; set; }
    }

}
