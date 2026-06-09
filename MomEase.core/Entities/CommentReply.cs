using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MomEase.core.Entities
{
    public class CommentReply
    {
        [Key]
        public int ReplyId { get; set; }

        [Required]
        public int CommentId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public string Text { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.Now.AddHours(1);
        public DateTime? UpdatedAt { get; set; }

        [ForeignKey("CommentId")]
        public virtual PostComments Comment { get; set; } = null!;

        [ForeignKey("UserId")]
        public virtual Users User { get; set; } = null!;
    }
}