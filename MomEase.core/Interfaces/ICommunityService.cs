using MomEase.core.DTOS.CommunityDTO;
using Microsoft.AspNetCore.Http;

namespace MomEase.core.Interfaces
{
    public interface ICommunityService
    {
        // ===== Posts =====
        Task<PostsPagedDto> GetAllPostsAsync(int pageNumber, int pageSize, int currentUserId);
        Task<CommunityPostDto> GetPostByIdAsync(int postId, int currentUserId);
        Task<CommunityPostDto> CreatePostAsync(int userId, CreatePostDto dto);
        Task<CommunityPostDto> UpdatePostAsync(int postId, int userId, UpdatePostDto dto);
        Task DeletePostAsync(int postId, int userId);
        Task<PostsPagedDto> GetPostsByUserIdAsync(int targetUserId, int pageNumber,
            int pageSize, int currentUserId);
        Task<PostsPagedDto> GetMyPostsAsync(int userId, int pageNumber, int pageSize);

        // ===== Comments =====
        Task<PostCommentDto> AddCommentAsync(int postId, int userId, CreateCommentDto dto);
        Task<List<PostCommentDto>> GetPostCommentsAsync(int postId);
        Task<PostCommentDto> GetCommentByIdAsync(int commentId, int postId);
        Task<PostCommentDto> UpdateCommentAsync(int commentId, int postId,
            int userId, UpdateCommentDto dto);
        Task DeleteCommentAsync(int commentId, int postId, int userId);

        // ===== Reactions =====
        Task<PostReactionDto> AddReactionAsync(int postId, int userId, AddReactionDto dto);
        Task<List<PostReactionDto>> GetPostReactionsAsync(int postId);
        Task<ReactionsCountDto> GetReactionsCountAsync(int postId);
        Task DeleteReactionAsync(int postId, int userId);
        Task<PostReactionDto> UpdateReactionAsync(int postId, int userId, UpdateReactionDto dto);

        // ===== Saved Posts =====
        Task<SavedPostDto> SavePostAsync(int postId, int userId);
        Task RemoveFromSavedAsync(int postId, int userId);
        Task<List<SavedPostDto>> GetSavedPostsAsync(int userId);

        // ===== Reports =====
        Task<PostReportDto> ReportPostAsync(int postId, int userId, CreateReportDto dto);
        // ===== Admin Reports =====
        Task<List<PostReportDto>> GetAllReportsAsync();
        Task<PostReportDto> GetReportByIdAsync(int reportId);
        Task<List<PostReportDto>> GetPendingReportsAsync();
        Task<List<PostReportDto>> GetReviewedReportsAsync();
        //Task<PostReportDto> ReviewReportAsync(int reportId, int adminId);
        Task<PostReportDto> ReviewReportAsync(int reportId, int adminId, ReviewReportDto dto);
    }
}