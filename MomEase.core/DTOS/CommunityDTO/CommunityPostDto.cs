using Microsoft.AspNetCore.Http;

namespace MomEase.core.DTOS.CommunityDTO
{
    public class CreatePostDto
    {
        public string? Text { get; set; }
        public List<IFormFile>? MediaFiles { get; set; }
    }

    public class UpdatePostDto
    {
        public string? Text { get; set; }

        // IDs الصور اللي عايزة تحذفيهم
        public List<int>? MediaIdsToDelete { get; set; }

        // صور جديدة عايزة تضيفيهم
        public List<IFormFile>? NewMediaFiles { get; set; }
    }

    public class PostMediaDto
    {
        public int MediaId { get; set; }
        public string MediaUrl { get; set; } = string.Empty;
        public string MediaType { get; set; } = string.Empty;
        public int Order { get; set; }
    }

    public class CommunityPostDto
    {
        public int PostId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string? UserPhoto { get; set; }
        public string? Text { get; set; }
        public List<PostMediaDto> Media { get; set; } = new();
        public int CommentsCount { get; set; }
        public int ReactionsCount { get; set; }
        public string? MyReaction { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class PostsPagedDto
    {
        public List<CommunityPostDto> Posts { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public bool HasNextPage { get; set; }
        public bool HasPreviousPage { get; set; }
    }
}