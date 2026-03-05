using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MomEase.core.Enums;

namespace MomEase.core.Entities
{
    public class PostMedia
    {
        [Key]
        public int MediaId { get; set; }

        [Required]
        public int PostId { get; set; }

        [Required]
        [MaxLength(500)]
        public string MediaUrl { get; set; } = string.Empty;

        [Required]
        public MediaType MediaType { get; set; }

        public int Order { get; set; } = 0;

        // Navigation Property
        [ForeignKey("PostId")]
        public virtual CommunityPosts Post { get; set; } = null!;
    }
}