using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using MomEase.core.Enums;

namespace MomEase.core.Entities
{
    public class CommunityPosts
    {
        [Key]
        public int PostId { get; set; }

        [Required]
        public int UserId { get; set; }

        public string? Text { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; }

        // Navigation Properties
        [ForeignKey("UserId")]
        public virtual Users User { get; set; } = null!;

        public virtual ICollection<PostMedia> PostMedia { get; set; }
        public virtual ICollection<PostComments> PostComments { get; set; }
        public virtual ICollection<PostReactions> PostReactions { get; set; }
        public virtual ICollection<PostReports> PostReports { get; set; }
        public virtual ICollection<SavedPosts> SavedPosts { get; set; }
    }
}