namespace MomEase.core.DTOS.CommunityDTO
{
    public class CreateReplyDto
    {
        public string Text { get; set; } = string.Empty;
    }

    public class UpdateReplyDto
    {
        public string Text { get; set; } = string.Empty;
    }

    public class CommentReplyDto
    {
        public int ReplyId { get; set; }
        public int CommentId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string? UserPhoto { get; set; }
        public string Text { get; set; } = string.Empty;
        public bool IsMyReply { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}