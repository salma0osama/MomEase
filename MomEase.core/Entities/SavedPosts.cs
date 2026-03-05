using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MomEase.core.Entities
{
    public class SavedPosts
    {
        [Key]
        public int SavedPostId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public int PostId { get; set; }

        [Required]
        public DateTime SavedAt { get; set; } = DateTime.Now;

        // Navigation Properties
        [ForeignKey("UserId")]
        public virtual Users User { get; set; } = null!;

        [ForeignKey("PostId")]
        public virtual CommunityPosts Post { get; set; } = null!;
    }
}