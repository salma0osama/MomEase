using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MomEase.core.Entities;
using MomEase.core.Interfaces;
using MomEase.infra.Data;

namespace MomEase.infra.Repositories
{
    public class CommunityRepository : ICommunityRepository
    {
        private readonly MomEaseDbContext _context;
        private readonly ILogger<CommunityRepository> _logger;

        public CommunityRepository(
            MomEaseDbContext context,
            ILogger<CommunityRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        // ===== Posts =====

        public async Task<(List<CommunityPosts> Posts, int TotalCount)> GetAllPostsAsync(
            int pageNumber, int pageSize)
        {
            try
            {
                var query = _context.CommunityPosts
                    .Include(p => p.User)
                    .ThenInclude(u => u.MotherProfile)
                    .Include(p => p.PostMedia)
                    .Include(p => p.PostComments)
                    .Include(p => p.PostReactions)
                    .Include(p => p.SavedPosts)
                    .OrderByDescending(p => p.CreatedAt);

                var totalCount = await query.CountAsync();

                var posts = await query
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return (posts, totalCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all posts");
                throw;
            }
        }

        public async Task<CommunityPosts?> GetPostByIdAsync(int postId)
        {
            try
            {
                return await _context.CommunityPosts
                    .Include(p => p.User)
                    .ThenInclude(u => u.MotherProfile)
                    .Include(p => p.PostMedia.OrderBy(m => m.Order))
                    .Include(p => p.PostComments)
                    .Include(p => p.PostReactions)
                    .FirstOrDefaultAsync(p => p.PostId == postId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving post {PostId}", postId);
                throw;
            }
        }

        public async Task<List<CommunityPosts>> GetPostsByUserIdAsync(int userId)
        {
            try
            {
                return await _context.CommunityPosts
                    .Include(p => p.User)
                    .ThenInclude(u => u.MotherProfile)
                    .Include(p => p.PostMedia.OrderBy(m => m.Order))
                    .Include(p => p.PostComments)
                    .Include(p => p.PostReactions)
                    .Where(p => p.UserId == userId)
                    .OrderByDescending(p => p.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving posts for user {UserId}", userId);
                throw;
            }
        }

        public async Task<CommunityPosts> AddPostAsync(CommunityPosts post)
        {
            try
            {
                await _context.CommunityPosts.AddAsync(post);
                await _context.SaveChangesAsync();
                return post;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding post");
                throw;
            }
        }

        public async Task<CommunityPosts> UpdatePostAsync(CommunityPosts post)
        {
            try
            {
                _context.CommunityPosts.Update(post);
                await _context.SaveChangesAsync();
                return post;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating post {PostId}", post.PostId);
                throw;
            }
        }

        public async Task DeletePostAsync(CommunityPosts post)
        {
            try
            {
                await _context.CommunityPosts
             .Where(p => p.PostId == post.PostId)
             .ExecuteDeleteAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting post {PostId}", post.PostId);
                throw;
            }
        }

        // ===== Media =====

        public async Task AddPostMediaAsync(List<PostMedia> mediaList)
        {
            try
            {
                await _context.PostMedias.AddRangeAsync(mediaList);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding post media");
                throw;
            }
        }

        public async Task DeletePostMediaAsync(int postId)
        {
            try
            {
                var media = await _context.PostMedias
                    .Where(m => m.PostId == postId)
                    .ToListAsync();

                _context.PostMedias.RemoveRange(media);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting media for post {PostId}", postId);
                throw;
            }
        }

        // ===== Comments =====

        public async Task<List<PostComments>> GetPostCommentsAsync(int postId)
        {
            try
            {
                return await _context.PostComments
                    .Include(c => c.User)
                        .ThenInclude(u => u.MotherProfile)
                    .Include(c => c.Replies)
                    .Include(c => c.Reactions)
                    .Where(c => c.PostId == postId)
                    .OrderByDescending(c => c.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving comments for post {PostId}", postId);
                throw;
            }
        }

        public async Task<PostComments?> GetCommentByIdAsync(int commentId)
        {
            try
            {
                return await _context.PostComments
                    .Include(c => c.User)
                    .ThenInclude(u => u.MotherProfile)
                    .Include(c => c.Replies)
                    .Include(c => c.Reactions)
                    .Include(c => c.Post)
                    .FirstOrDefaultAsync(c => c.CommentId == commentId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving comment {CommentId}", commentId);
                throw;
            }
        }

        public async Task<PostComments> AddCommentAsync(PostComments comment)
        {
            try
            {
                await _context.PostComments.AddAsync(comment);
                await _context.SaveChangesAsync();
                return comment;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding comment");
                throw;
            }
        }

        public async Task<PostComments> UpdateCommentAsync(PostComments comment)
        {
            try
            {
                _context.PostComments.Update(comment);
                await _context.SaveChangesAsync();
                return comment;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating comment {CommentId}", comment.CommentId);
                throw;
            }
        }

        public async Task DeleteCommentAsync(PostComments comment)
        {
            try
            {
                _context.PostComments.Remove(comment);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting comment {CommentId}", comment.CommentId);
                throw;
            }
        }

        // ===== Reactions =====

        public async Task<List<PostReactions>> GetPostReactionsAsync(int postId)
        {
            try
            {
                return await _context.PostReactions
                    .Include(r => r.User)
                    .ThenInclude(u => u.MotherProfile)
                    .Where(r => r.PostId == postId)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving reactions for post {PostId}", postId);
                throw;
            }
        }

        public async Task<PostReactions?> GetUserReactionAsync(int postId, int userId)
        {
            try
            {
                return await _context.PostReactions
                    .FirstOrDefaultAsync(r => r.PostId == postId && r.UserId == userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving reaction for post {PostId}", postId);
                throw;
            }
        }

        public async Task<PostReactions> AddReactionAsync(PostReactions reaction)
        {
            try
            {
                await _context.PostReactions.AddAsync(reaction);
                await _context.SaveChangesAsync();
                return reaction;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding reaction");
                throw;
            }
        }

        public async Task<PostReactions> UpdateReactionAsync(PostReactions reaction)
        {
            try
            {
                _context.PostReactions.Update(reaction);
                await _context.SaveChangesAsync();
                return reaction;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating reaction {ReactionId}", reaction.ReactionId);
                throw;
            }
        }

        public async Task DeleteReactionAsync(PostReactions reaction)
        {
            try
            {
                _context.PostReactions.Remove(reaction);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting reaction {ReactionId}", reaction.ReactionId);
                throw;
            }
        }
        // ===== Saved Posts =====

        public async Task<SavedPosts?> GetSavedPostAsync(int postId, int userId)
        {
            try
            {
                return await _context.SavedPosts
                    .FirstOrDefaultAsync(s => s.PostId == postId && s.UserId == userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving saved post {PostId}", postId);
                throw;
            }
        }

        public async Task<SavedPosts> SavePostAsync(SavedPosts savedPost)
        {
            try
            {
                await _context.SavedPosts.AddAsync(savedPost);
                await _context.SaveChangesAsync();
                return savedPost;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving post {PostId}", savedPost.PostId);
                throw;
            }
        }

        public async Task RemoveSavedPostAsync(SavedPosts savedPost)
        {
            try
            {
                _context.SavedPosts.Remove(savedPost);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing saved post {PostId}", savedPost.PostId);
                throw;
            }
        }

        public async Task<List<SavedPosts>> GetUserSavedPostsAsync(int userId)
        {
            try
            {
                return await _context.SavedPosts
                    .Include(s => s.Post)
                        .ThenInclude(p => p.User)
                        .ThenInclude(u => u.MotherProfile)
                    .Include(s => s.Post)
                        .ThenInclude(p => p.PostMedia)
                    .Include(s => s.Post)
                        .ThenInclude(p => p.PostComments)
                    .Include(s => s.Post)
                        .ThenInclude(p => p.PostReactions)
                          //.ThenInclude(p => p.SavedPosts)
                    .Where(s => s.UserId == userId)
                    .OrderByDescending(s => s.SavedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving saved posts for user {UserId}", userId);
                throw;
            }
        }

        // ===== Reports =====

        public async Task<PostReports?> GetReportAsync(int postId, int userId)
        {
            try
            {
                return await _context.PostReports
                    .FirstOrDefaultAsync(r => r.PostId == postId && r.ReporterId == userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving report for post {PostId}", postId);
                throw;
            }
        }

        public async Task<PostReports> AddReportAsync(PostReports report)
        {
            try
            {
                await _context.PostReports.AddAsync(report);
                await _context.SaveChangesAsync();
                return report;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding report for post {PostId}", report.PostId);
                throw;
            }
        }
        public async Task<PostMedia?> GetPostMediaByIdAsync(int mediaId)
        {
            try
            {
                return await _context.PostMedias
                    .FirstOrDefaultAsync(m => m.MediaId == mediaId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving media {MediaId}", mediaId);
                throw;
            }
        }

        public async Task DeleteSingleMediaAsync(PostMedia media)
        {
            try
            {
                _context.PostMedias.Remove(media);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting media {MediaId}", media.MediaId);
                throw;
            }
        }
        public async Task<List<PostReports>> GetAllReportsAsync()
        {
            try
            {
                return await _context.PostReports
                    .Include(r => r.Reporter)
                    .Include(r => r.Post)
                    .Include(r => r.ReviewedBy)
                    .OrderByDescending(r => r.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all reports");
                throw;
            }
        }

        public async Task<PostReports?> GetReportByIdAsync(int reportId)
        {
            try
            {
                return await _context.PostReports
                    .Include(r => r.Reporter)
                    //.Include(r => r.Post)
                    .Include(r => r.ReviewedBy)
                     .AsNoTracking()        

                     .FirstOrDefaultAsync(r => r.ReportId == reportId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving report {ReportId}", reportId);
                throw;
            }
        }

        public async Task<List<PostReports>> GetPendingReportsAsync()
        {
            try
            {
                return await _context.PostReports
                    .Include(r => r.Reporter)
                    .Include(r => r.Post)
                    .Where(r => r.ReviewedById == null)
                    .OrderByDescending(r => r.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving pending reports");
                throw;
            }
        }

        public async Task<List<PostReports>> GetReviewedReportsAsync()
        {
            try
            {
                return await _context.PostReports
                    .Include(r => r.Reporter)
                    .Include(r => r.Post)
                    .Include(r => r.ReviewedBy)
                    .Where(r => r.ReviewedById != null)
                    .OrderByDescending(r => r.ReviewedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving reviewed reports");
                throw;
            }
        }

        public async Task<PostReports> UpdateReportAsync(PostReports report)
        {
            try
            {
                _context.PostReports.Attach(report);
                _context.Entry(report).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                return report;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating report {ReportId}", report.ReportId);
                throw;
            }
        }
        public async Task<CommunityPosts?> GetPostByIdNoTrackingAsync(int postId)
        {
            try
            {
                return await _context.CommunityPosts
                    .Include(p => p.User)
                    .Include(p => p.PostMedia.OrderBy(m => m.Order))
                    .Include(p => p.PostComments)
                    .Include(p => p.PostReactions)
                    .AsNoTracking()   
                    .FirstOrDefaultAsync(p => p.PostId == postId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving post {PostId}", postId);
                throw;
            }
        }
        public async Task UpdateReportFields(
    int reportId, int adminId, string action, string? adminNote)
        {
            try
            {
                await _context.PostReports
                    .Where(r => r.ReportId == reportId)
                    .ExecuteUpdateAsync(s => s
                        .SetProperty(r => r.ReviewedById, adminId)
                        .SetProperty(r => r.ReviewedAt, DateTime.Now.AddHours(1))
                        .SetProperty(r => r.Action, action)
                        .SetProperty(r => r.AdminNote, adminNote));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating report fields {ReportId}", reportId);
                throw;
            }
        }
        // ===== Comment Replies =====

        public async Task<CommentReply> AddReplyAsync(CommentReply reply)
        {
            try
            {
                await _context.CommentReplies.AddAsync(reply);
                await _context.SaveChangesAsync();
                return reply;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding reply");
                throw;
            }
        }

        public async Task<CommentReply?> GetReplyByIdAsync(int replyId)
        {
            try
            {
                return await _context.CommentReplies
                    .Include(r => r.User)
                        .ThenInclude(u => u.MotherProfile)
                    .FirstOrDefaultAsync(r => r.ReplyId == replyId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving reply {ReplyId}", replyId);
                throw;
            }
        }

        public async Task<List<CommentReply>> GetCommentRepliesAsync(int commentId)
        {
            try
            {
                return await _context.CommentReplies
                    .Include(r => r.User)
                        .ThenInclude(u => u.MotherProfile)
                    .Where(r => r.CommentId == commentId)
                    .OrderBy(r => r.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving replies for comment {CommentId}", commentId);
                throw;
            }
        }

        public async Task<CommentReply> UpdateReplyAsync(CommentReply reply)
        {
            try
            {
                _context.CommentReplies.Update(reply);
                await _context.SaveChangesAsync();
                return reply;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating reply {ReplyId}", reply.ReplyId);
                throw;
            }
        }

        public async Task DeleteReplyAsync(CommentReply reply)
        {
            try
            {
                _context.CommentReplies.Remove(reply);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting reply {ReplyId}", reply.ReplyId);
                throw;
            }
        }

        // ===== Comment Reactions =====

        public async Task<CommentReaction> AddCommentReactionAsync(CommentReaction reaction)
        {
            try
            {
                await _context.CommentReactions.AddAsync(reaction);
                await _context.SaveChangesAsync();
                return reaction;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding comment reaction");
                throw;
            }
        }

        public async Task<CommentReaction?> GetUserCommentReactionAsync(int commentId, int userId)
        {
            try
            {
                return await _context.CommentReactions
                    .FirstOrDefaultAsync(r => r.CommentId == commentId && r.UserId == userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving reaction for comment {CommentId}", commentId);
                throw;
            }
        }

        public async Task<List<CommentReaction>> GetCommentReactionsAsync(int commentId)
        {
            try
            {
                return await _context.CommentReactions
                    .Include(r => r.User)
                        .ThenInclude(u => u.MotherProfile)
                    .Where(r => r.CommentId == commentId)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving reactions for comment {CommentId}", commentId);
                throw;
            }
        }

        public async Task<CommentReaction> UpdateCommentReactionAsync(CommentReaction reaction)
        {
            try
            {
                _context.CommentReactions.Update(reaction);
                await _context.SaveChangesAsync();
                return reaction;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating comment reaction {ReactionId}", reaction.ReactionId);
                throw;
            }
        }

        public async Task DeleteCommentReactionAsync(CommentReaction reaction)
        {
            try
            {
                _context.CommentReactions.Remove(reaction);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting comment reaction {ReactionId}", reaction.ReactionId);
                throw;
            }
        }
        public async Task<CommentReaction?> GetCommentReactionByIdAsync(int reactionId)
        {
            try
            {
                return await _context.CommentReactions
                    .Include(r => r.User)
                        .ThenInclude(u => u.MotherProfile)
                    .FirstOrDefaultAsync(r => r.ReactionId == reactionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving comment reaction {ReactionId}", reactionId);
                throw;
            }
        }
    }
}