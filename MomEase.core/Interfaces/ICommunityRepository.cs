using MomEase.core.Entities;

namespace MomEase.core.Interfaces
{
    public interface ICommunityRepository
    {
        // ===== Posts =====
        Task<(List<CommunityPosts> Posts, int TotalCount)> GetAllPostsAsync(
            int pageNumber, int pageSize);
        Task<CommunityPosts?> GetPostByIdAsync(int postId);
        Task<List<CommunityPosts>> GetPostsByUserIdAsync(int userId);
        Task<CommunityPosts> AddPostAsync(CommunityPosts post);
        Task<CommunityPosts> UpdatePostAsync(CommunityPosts post);
        Task DeletePostAsync(CommunityPosts post);

        // ===== Media =====
        Task AddPostMediaAsync(List<PostMedia> mediaList);
        Task DeletePostMediaAsync(int postId);

        // ===== Comments =====
        Task<List<PostComments>> GetPostCommentsAsync(int postId);
        Task<PostComments?> GetCommentByIdAsync(int commentId);
        Task<PostComments> AddCommentAsync(PostComments comment);
        Task<PostComments> UpdateCommentAsync(PostComments comment);
        Task DeleteCommentAsync(PostComments comment);

        // ===== Reactions =====
        Task<List<PostReactions>> GetPostReactionsAsync(int postId);
        Task<PostReactions?> GetUserReactionAsync(int postId, int userId);
        Task<PostReactions> AddReactionAsync(PostReactions reaction);
        Task<PostReactions> UpdateReactionAsync(PostReactions reaction);
        Task DeleteReactionAsync(PostReactions reaction);
        // ===== Saved Posts =====
        Task<SavedPosts?> GetSavedPostAsync(int postId, int userId);
        Task<SavedPosts> SavePostAsync(SavedPosts savedPost);
        Task RemoveSavedPostAsync(SavedPosts savedPost);
        Task<List<SavedPosts>> GetUserSavedPostsAsync(int userId);

        // ===== Reports =====
        Task<PostReports?> GetReportAsync(int postId, int userId);
        Task<PostReports> AddReportAsync(PostReports report);
        Task<PostMedia?> GetPostMediaByIdAsync(int mediaId);
        Task DeleteSingleMediaAsync(PostMedia media);
        // ===== Admin Reports =====
        Task<List<PostReports>> GetAllReportsAsync();
        Task<PostReports?> GetReportByIdAsync(int reportId);
        Task<List<PostReports>> GetPendingReportsAsync();
        Task<List<PostReports>> GetReviewedReportsAsync();
        Task<PostReports> UpdateReportAsync(PostReports report);

        Task<CommunityPosts?> GetPostByIdNoTrackingAsync(int postId);
        Task UpdateReportFields(int reportId, int adminId, string action, string? adminNote);
    }
}