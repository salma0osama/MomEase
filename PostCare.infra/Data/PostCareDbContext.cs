using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using PostCare.core.Entities;

namespace PostCare.infra.Data
{
    public class PostCareDbContext : DbContext
    {
        public PostCareDbContext(DbContextOptions<PostCareDbContext> options)
            : base(options)
        {
        }

        // ===== USER & AUTH =====
        public DbSet<Users> Users { get; set; }

        // ===== CHILD =====
        public DbSet<Child> Children { get; set; }
        public DbSet<ChildVaccination> ChildVaccinations { get; set; }
        public DbSet<Vaccinations> Vaccinations { get; set; }

        // ===== TRACKING =====
        public DbSet<ChildFeedingRecord> ChildFeedingRecords { get; set; }
        public DbSet<FeedingReference> FeedingReferences { get; set; }
        public DbSet<ChildSleepRecord> ChildSleepRecords { get; set; }
        public DbSet<SleepReference> SleepReferences { get; set; }
        public DbSet<GrowthRecords> GrowthRecords { get; set; }
        public DbSet<GrowthPercentileReference> GrowthPercentileReferences { get; set; }

        // ===== ASSESSMENT =====
        public DbSet<Assessment> Assessments { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<AnswerOption> AnswerOptions { get; set; }
        public DbSet<UserResponse> UserResponses { get; set; }
        public DbSet<AssessmentResult> AssessmentResults { get; set; }
        public DbSet<ScoreLevel> ScoreLevels { get; set; }

        // ===== COMMUNITY =====
        public DbSet<CommunityPosts> CommunityPosts { get; set; }
        public DbSet<PostComments> PostComments { get; set; }
        public DbSet<PostReactions> PostReactions { get; set; }
        public DbSet<PostReports> PostReports { get; set; }

        // ===== ARTICLES =====
        public DbSet<Articles> Articles { get; set; }
        public DbSet<ArticleCategories> ArticleCategories { get; set; }

        // ===== AI =====
        public DbSet<SkinAnalyses> SkinAnalyses { get; set; }
        public DbSet<CryAnalyses> CryAnalyses { get; set; }
        public DbSet<Diseases> Diseases { get; set; }
        public DbSet<CryReasons> CryReasons { get; set; }

        // ===== CHAT & NOTIFICATIONS =====
        public DbSet<ChatBot> ChatBots { get; set; }
        public DbSet<Notifications> Notifications { get; set; }
    }
}
