namespace MomEase.core.DTOS.CommunityDTO
{
    public class CreateCommentDto
    {
        public string Text { get; set; } = string.Empty;
    }

    public class UpdateCommentDto
    {
        public string Text { get; set; } = string.Empty;
    }

    public class PostCommentDto
    {
        public int CommentId { get; set; }
        public int PostId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string? UserPhoto { get; set; }
        public string Text { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsMyComment { get; set; }
        public bool CanDelete { get; set; } // صاحب الكومنت أو صاحب البوست
        public int RepliesCount { get; set; }
        public int ReactionsCount { get; set; }
        public string? MyReaction { get; set; }
    }
}