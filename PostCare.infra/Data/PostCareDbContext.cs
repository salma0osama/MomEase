using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using PostCare.core.Entities;
using PostCare.core.Enums;

namespace PostCare.infra.Data
{
    public class PostCareDbContext : DbContext
    {
        public PostCareDbContext(DbContextOptions<PostCareDbContext> options)
            : base(options)
        {
        }

            // ============================
            // DbSets
            // ============================
            public DbSet<Users> Users { get; set; }
            public DbSet<MotherProfile> MotherProfiles { get; set; }
            public DbSet<Child> Children { get; set; }

            public DbSet<GrowthPercentileReference> GrowthPercentileReferences { get; set; }
            public DbSet<GrowthRecords> GrowthRecords { get; set; }
            public DbSet<GrowthReports> GrowthReports { get; set; }

            public DbSet<SleepReference> SleepReferences { get; set; }
            public DbSet<ChildSleepRecord> ChildSleepRecords { get; set; }

            public DbSet<FeedingReference> FeedingReferences { get; set; }
            public DbSet<ChildFeedingRecord> ChildFeedingRecords { get; set; }

            public DbSet<Vaccinations> Vaccinations { get; set; }
            public DbSet<ChildVaccination> ChildVaccinations { get; set; }

            public DbSet<Diseases> Diseases { get; set; }
            public DbSet<SkinAnalyses> SkinAnalyses { get; set; }

            public DbSet<CryReasons> CryReasons { get; set; }
            public DbSet<CryAnalyses> CryAnalyses { get; set; }

            public DbSet<Assessment> Assessments { get; set; }
            public DbSet<ScoreLevel> ScoreLevels { get; set; }
            public DbSet<Question> Questions { get; set; }
            public DbSet<AnswerOption> AnswerOptions { get; set; }
            public DbSet<AssessmentResult> AssessmentResults { get; set; }
            public DbSet<UserResponse> UserResponses { get; set; }

            public DbSet<CommunityPosts> CommunityPosts { get; set; }
            public DbSet<PostComments> PostComments { get; set; }
            public DbSet<PostReactions> PostReactions { get; set; }
            public DbSet<PostReports> PostReports { get; set; }

            public DbSet<ArticleCategories> ArticleCategories { get; set; }
            public DbSet<Articles> Articles { get; set; }

            public DbSet<Notifications> Notifications { get; set; }

            public DbSet<ChatBot> ChatBots { get; set; }
            public DbSet<ChatMessages> ChatMessages { get; set; }

            public DbSet<RefreshToken> RefreshTokens { get; set; }
            public DbSet<PasswordResetToken> PasswordResetTokens { get; set; }
        // ============================
        // Fluent API
        // ============================
        protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                base.OnModelCreating(modelBuilder);

                // ---------- Users ----------
                modelBuilder.Entity<Users>(entity =>
                {
                    entity.HasKey(e => e.UserId);
                    entity.HasIndex(e => e.Email).IsUnique();
                    entity.Property(e => e.Role).HasDefaultValue(Role.MOTHER);
                    entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");
                });

                // ---------- MotherProfile ----------
                modelBuilder.Entity<MotherProfile>(entity =>
                {
                    entity.HasKey(e => e.MotherId);

                    entity.HasOne(e => e.User)
                        .WithOne(u => u.MotherProfile)
                        .HasForeignKey<MotherProfile>(e => e.UserId)
                        .OnDelete(DeleteBehavior.Cascade);
                });

                // ---------- Children ----------
                modelBuilder.Entity<Child>(entity =>
                {
                    entity.HasKey(e => e.ChildId);

                    entity.HasOne(e => e.User)
                        .WithMany(u => u.Children)
                        .HasForeignKey(e => e.UserId)
                        .OnDelete(DeleteBehavior.Cascade);
                });

                // ---------- GrowthPercentileReference ----------
                modelBuilder.Entity<GrowthPercentileReference>(entity =>
                {
                    entity.HasKey(e => e.RefId);
                    entity.HasIndex(e => new { e.Gender, e.AgeMonths, e.Metric_Type }).IsUnique();
                });

                // ---------- ChildVaccination ----------
                modelBuilder.Entity<ChildVaccination>(entity =>
                {
                    entity.HasKey(e => e.ChildVaccineId);

                    entity.HasOne(e => e.Child)
                        .WithMany(c => c.ChildVaccinations)
                        .HasForeignKey(e => e.ChildId)
                        .OnDelete(DeleteBehavior.Cascade);

                    entity.HasOne(e => e.Vaccination)
                        .WithMany(v => v.ChildVaccinations)
                        .HasForeignKey(e => e.ScheduleId)
                        .OnDelete(DeleteBehavior.Restrict);
                });

                // ---------- SkinAnalyses ----------
                modelBuilder.Entity<SkinAnalyses>(entity =>
                {
                    entity.HasKey(e => e.SkinanalysisId);

                    entity.HasOne(e => e.User)
                        .WithMany(u => u.SkinAnalyses)
                        .HasForeignKey(e => e.UserId)
                        .OnDelete(DeleteBehavior.NoAction);

                    entity.HasOne(e => e.Child)
                        .WithMany(c => c.SkinAnalyses)
                        .HasForeignKey(e => e.ChildId)
                        .OnDelete(DeleteBehavior.Restrict);

                    entity.HasOne(e => e.Disease)
                        .WithMany(d => d.SkinAnalyses)
                        .HasForeignKey(e => e.DiseaseId)
                        .OnDelete(DeleteBehavior.SetNull);
                });

                // ---------- CryAnalyses ----------
                modelBuilder.Entity<CryAnalyses>(entity =>
                {
                    entity.HasKey(e => e.CryId);

                    entity.HasOne(e => e.User)
                        .WithMany(u => u.CryAnalyses)
                        .HasForeignKey(e => e.UserId)
                        .OnDelete(DeleteBehavior.NoAction);

                    entity.HasOne(e => e.Child)
                        .WithMany(c => c.CryAnalyses)
                        .HasForeignKey(e => e.ChildId)
                        .OnDelete(DeleteBehavior.Restrict);

                    entity.HasOne(e => e.CryReason)
                        .WithMany(cr => cr.CryAnalyses)
                        .HasForeignKey(e => e.CryreasonId)
                        .OnDelete(DeleteBehavior.SetNull);
                });

                // ---------- AssessmentResult ----------
                modelBuilder.Entity<AssessmentResult>(entity =>
                {
                    entity.HasKey(e => e.ResultId);

                    entity.HasOne(e => e.User)
                        .WithMany(u => u.AssessmentResults)
                        .HasForeignKey(e => e.UserId)
                        .OnDelete(DeleteBehavior.NoAction);

                    entity.HasOne(e => e.Assessment)
                        .WithMany(a => a.AssessmentResults)
                        .HasForeignKey(e => e.AssessmentId)
                        .OnDelete(DeleteBehavior.Restrict);

                    entity.HasOne(e => e.ScoreLevel)
                        .WithMany(sl => sl.AssessmentResults)
                        .HasForeignKey(e => e.LevelId)
                        .OnDelete(DeleteBehavior.NoAction);
                });

                // ---------- UserResponse ----------
                modelBuilder.Entity<UserResponse>(entity =>
                {
                    entity.HasKey(e => e.ResponseId);

                    entity.HasOne(e => e.AssessmentResult)
                        .WithMany(ar => ar.UserResponses)
                        .HasForeignKey(e => e.ResultId)
                        .OnDelete(DeleteBehavior.Cascade);

                    entity.HasOne(e => e.Question)
                        .WithMany(q => q.UserResponses)
                        .HasForeignKey(e => e.QuestionId)
                        .OnDelete(DeleteBehavior.Restrict);

                    entity.HasOne(e => e.AnswerOption)
                        .WithMany(ao => ao.UserResponses)
                        .HasForeignKey(e => e.OptionId)
                        .OnDelete(DeleteBehavior.Restrict);
                });

                // ---------- Community ----------
                modelBuilder.Entity<CommunityPosts>(entity =>
                {
                    entity.HasKey(e => e.PostId);

                    entity.HasOne(e => e.User)
                        .WithMany(u => u.CommunityPosts)
                        .HasForeignKey(e => e.UserId)
                        .OnDelete(DeleteBehavior.NoAction);
                });

                modelBuilder.Entity<PostComments>(entity =>
                {
                    entity.HasKey(e => e.CommentId);

                    entity.HasOne(e => e.Post)
                        .WithMany(p => p.PostComments)
                        .HasForeignKey(e => e.PostId)
                        .OnDelete(DeleteBehavior.Cascade);

                    entity.HasOne(e => e.User)
                        .WithMany(u => u.PostComments)
                        .HasForeignKey(e => e.UserId)
                        .OnDelete(DeleteBehavior.NoAction);
                });

                modelBuilder.Entity<PostReactions>(entity =>
                {
                    entity.HasKey(e => e.ReactionId);
                    entity.HasIndex(e => new { e.PostId, e.UserId }).IsUnique();

                    entity.HasOne(e => e.Post)
                        .WithMany(p => p.PostReactions)
                        .HasForeignKey(e => e.PostId)
                        .OnDelete(DeleteBehavior.Cascade);

                    entity.HasOne(e => e.User)
                        .WithMany(u => u.PostReactions)
                        .HasForeignKey(e => e.UserId)
                        .OnDelete(DeleteBehavior.NoAction);
                });

                modelBuilder.Entity<PostReports>(entity =>
                {
                    entity.HasKey(e => e.ReportId);

                    entity.HasOne(e => e.Post)
                        .WithMany(p => p.PostReports)
                        .HasForeignKey(e => e.PostId)
                        .OnDelete(DeleteBehavior.Cascade);

                    entity.HasOne(e => e.Reporter)
                        .WithMany(u => u.SubmittedReports)
                        .HasForeignKey(e => e.ReporterId)
                        .OnDelete(DeleteBehavior.NoAction);

                    entity.HasOne(e => e.ReviewedBy)
                        .WithMany(u => u.ReviewedReports)
                        .HasForeignKey(e => e.ReviewedById)
                        .OnDelete(DeleteBehavior.NoAction);
                });

                // ---------- Articles ----------
                modelBuilder.Entity<Articles>(entity =>
                {
                    entity.HasKey(e => e.ArticleId);

                    entity.HasOne(e => e.Category)
                        .WithMany(c => c.Articles)
                        .HasForeignKey(e => e.CategoryId)
                        .OnDelete(DeleteBehavior.Restrict);
                });

                // ---------- Notifications ----------
                modelBuilder.Entity<Notifications>(entity =>
                {
                    entity.HasKey(e => e.NotificationId);
                    entity.Property(e => e.IsRead).HasDefaultValue(false);
                    entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");

                    entity.HasOne(e => e.User)
                        .WithMany(u => u.Notifications)
                        .HasForeignKey(e => e.UserId)
                        .OnDelete(DeleteBehavior.NoAction);
                });

                // ---------- ChatBot ----------
                modelBuilder.Entity<ChatBot>(entity =>
                {
                    entity.HasKey(e => e.ChatId);

                    entity.HasOne(e => e.User)
                        .WithMany(u => u.ChatBots)
                        .HasForeignKey(e => e.UserId)
                        .OnDelete(DeleteBehavior.NoAction);
                });
              //---------- RefreshToken---------- 
            modelBuilder.Entity<RefreshToken>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Token).IsUnique();

                entity.HasOne(e => e.User)
                    .WithMany(u => u.RefreshTokens)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
        }
    }




