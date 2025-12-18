using PostCare.core.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PostCare.core.Entities
{
    public class Users
    {
        public int UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Phone { get; set; }
        public int? Age { get; set; }
        public Role Role { get; set; }
        public DateTime CreatedAt { get; set; }

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
        public virtual ICollection<ArticleCategories> ArticleCategories { get; set; }
        public ICollection<PostReports> SubmittedReports { get; set; }
        public ICollection<PostReports> ReviewedReports { get; set; }

        public virtual ICollection<Notifications> Notifications { get; set; }
    }
}
