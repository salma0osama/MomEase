using MomEase.core.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MomEase.core.Entities
{
    public class CommentReaction
    {
        [Key]
        public int ReactionId { get; set; }

        [Required]
        public int CommentId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public ReactionType ReactionType { get; set; }

        [ForeignKey("CommentId")]
        public virtual PostComments Comment { get; set; } = null!;

        [ForeignKey("UserId")]
        public virtual Users User { get; set; } = null!;
    }
}