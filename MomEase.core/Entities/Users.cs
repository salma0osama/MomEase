using MomEase.core.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MomEase.core.Entities
{
    public class Users
    {
        [Key]
        public int UserId { get; set; }

        [Required, MaxLength(100)]
        public string FirstName { get; set; }

        [Required, MaxLength(100)]
        public string LastName { get; set; }

        [Required, MaxLength(255), EmailAddress]
        public string Email { get; set; }

        [Required, MaxLength(255)]
        public string Password { get; set; }

        [MaxLength(20)]
        public string Phone { get; set; }

        public int? Age { get; set; }

        [Required]
        public Role Role { get; set; } = Role.MOTHER;

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public bool IsEmailVerified { get; set; } = false;
        public string? EmailVerificationToken { get; set; }
        public DateTime? EmailVerificationTokenExpiry { get; set; }

        public string? GoogleId { get; set; }
        public bool IsExternalAuth { get; set; } = false;

        // Navigation Properties
        public virtual MotherProfile MotherProfile { get; set; }
        public virtual ICollection<Child> Children { get; set; }
        public virtual ICollection<SkinAnalyses> SkinAnalyses { get; set; }
        public virtual ICollection<CryAnalyses> CryAnalyses { get; set; }
        public virtual ICollection<ChatBot> ChatBots { get; set; }
        public virtual ICollection<AssessmentResult> AssessmentResults { get; set; }
        public virtual ICollection<CommunityPosts> CommunityPosts { get; set; }
        public virtual ICollection<PostComments> PostComments { get; set; }
        public virtual ICollection<PostReactions> PostReactions { get; set; }
        public virtual ICollection<PostReports> SubmittedReports { get; set; }
        public virtual ICollection<PostReports> ReviewedReports { get; set; }
        public virtual ICollection<Notifications> Notifications { get; set; }
        public virtual ICollection<UserResponse> UserResponses { get; set; }
        public virtual ICollection<ArticleCategories> ArticleCategories { get; set; }

        public virtual ICollection<RefreshToken> RefreshTokens { get; set; }
        public virtual ICollection<SearchHistory> SearchHistories { get; set; }
        public virtual ICollection<SavedArticles> SavedArticles { get; set; }
        public virtual ICollection<SavedPosts> SavedPosts { get; set; }
    }
}
