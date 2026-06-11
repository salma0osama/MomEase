using Microsoft.EntityFrameworkCore;
using MomEase.core.Entities;
using MomEase.core.Interfaces;
using MomEase.infra.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MomEase.infra.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly MomEaseDbContext _context;

        public UserRepository(MomEaseDbContext context)
        {
            _context = context;
        }

        public async Task<Users> GetByIdAsync(int userId)
        {
            return await _context.Users
                .Include(u => u.MotherProfile)
                .Include(u => u.Children)
                .FirstOrDefaultAsync(u => u.UserId == userId);
        }

        public async Task<Users> GetByEmailAsync(string email)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<IEnumerable<Users>> GetAllAsync()
        {
            return await _context.Users
                .Include(u => u.MotherProfile)
                .Include(u => u.Children)
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync();
        }

        public async Task<Users> UpdateAsync(Users user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<bool> DeleteAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return false;

            // 1. SkinAnalyses (NoAction)
            var skinAnalyses = await _context.SkinAnalyses
                .Where(s => s.UserId == userId).ToListAsync();
            _context.SkinAnalyses.RemoveRange(skinAnalyses);

            // 2. CryAnalyses (NoAction)
            var cryAnalyses = await _context.CryAnalyses
                .Where(c => c.UserId == userId).ToListAsync();
            _context.CryAnalyses.RemoveRange(cryAnalyses);

            // 3. AssessmentResults (NoAction) + UserResponses بتتمسح cascade منها
            var assessmentResults = await _context.AssessmentResults
                .Where(a => a.UserId == userId).ToListAsync();
            _context.AssessmentResults.RemoveRange(assessmentResults);

            // 4. CommunityPosts (NoAction) + Comments/Reactions/Media/SavedPosts بتتمسح cascade
            var posts = await _context.CommunityPosts
                .Where(p => p.UserId == userId).ToListAsync();
            _context.CommunityPosts.RemoveRange(posts);

            // 5. PostComments (NoAction)
            var comments = await _context.PostComments
                .Where(c => c.UserId == userId).ToListAsync();
            _context.PostComments.RemoveRange(comments);

            // 6. PostReactions (NoAction)
            var reactions = await _context.PostReactions
                .Where(r => r.UserId == userId).ToListAsync();
            _context.PostReactions.RemoveRange(reactions);

            // 7. PostReports (NoAction) - reporter و reviewer
            var submittedReports = await _context.PostReports
                .Where(r => r.ReporterId == userId).ToListAsync();
            _context.PostReports.RemoveRange(submittedReports);

            var reviewedReports = await _context.PostReports
                .Where(r => r.ReviewedById == userId).ToListAsync();
            _context.PostReports.RemoveRange(reviewedReports);

            // 8. Notifications (NoAction)
            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId).ToListAsync();
            _context.Notifications.RemoveRange(notifications);

            // 9. ChatBots (NoAction)
            var chatBots = await _context.ChatBots
                .Where(c => c.UserId == userId).ToListAsync();
            _context.ChatBots.RemoveRange(chatBots);

            // 10. SavedPosts (NoAction)
            var savedPosts = await _context.SavedPosts
                .Where(s => s.UserId == userId).ToListAsync();
            _context.SavedPosts.RemoveRange(savedPosts);

            // 11. CommentReplies (NoAction)
            var replies = await _context.CommentReplies
                .Where(r => r.UserId == userId).ToListAsync();
            _context.CommentReplies.RemoveRange(replies);

            // 12. CommentReactions (NoAction)
            var commentReactions = await _context.CommentReactions
                .Where(r => r.UserId == userId).ToListAsync();
            _context.CommentReactions.RemoveRange(commentReactions);

            // 13. DailyTrackingReminders (Restrict)
            var reminders = await _context.DailyTrackingReminders
                .Where(d => d.UserId == userId).ToListAsync();
            _context.DailyTrackingReminders.RemoveRange(reminders);

            // 14. SearchHistories لو موجودة
            var searches = await _context.SearchHistories
                .Where(s => s.UserId == userId).ToListAsync();
            _context.SearchHistories.RemoveRange(searches);

            // 15. SavedArticles
            var savedArticles = await _context.SavedArticles
                .Where(s => s.UserId == userId).ToListAsync();
            _context.SavedArticles.RemoveRange(savedArticles);

            // 16. DeviceTokens
            var deviceTokens = await _context.DeviceTokens
                .Where(d => d.UserId == userId).ToListAsync();
            _context.DeviceTokens.RemoveRange(deviceTokens);

            // 17. PasswordResetTokens
            var resetTokens = await _context.PasswordResetTokens
                .Where(p => p.UserId == userId).ToListAsync();
            _context.PasswordResetTokens.RemoveRange(resetTokens);

            // 18. MentalHealthFollowUps
            var followUps = await _context.MentalHealthFollowUps
                .Where(m => m.UserId == userId).ToListAsync();
            _context.MentalHealthFollowUps.RemoveRange(followUps);

            // 19. SentMentalHealthTips
            var sentTips = await _context.SentMentalHealthTips
                .Where(s => s.UserId == userId).ToListAsync();
            _context.SentMentalHealthTips.RemoveRange(sentTips);

            // الباقي بيتمسح Cascade تلقائي من الـ DbContext:
            // MotherProfile ← Cascade
            // Children ← Cascade (وبيجرّوا معاهم GrowthRecords, Vaccinations, إلخ)
            // RefreshTokens ← Cascade

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(int userId)
        {
            return await _context.Users.AnyAsync(u => u.UserId == userId);
        }

        public async Task<int> GetChildrenCountAsync(int userId)
        {
            return await _context.Children
                .CountAsync(c => c.UserId == userId);
        }

        public async Task<bool> HasMotherProfileAsync(int userId)
        {
            return await _context.MotherProfiles
                .AnyAsync(m => m.UserId == userId);
        }
    }
}
