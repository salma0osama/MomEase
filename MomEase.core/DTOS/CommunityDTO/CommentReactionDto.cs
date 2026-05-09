namespace MomEase.core.DTOS.CommunityDTO
{
    public class AddCommentReactionDto
    {
        public string ReactionType { get; set; } = string.Empty;
    }

    public class UpdateCommentReactionDto
    {
        public string ReactionType { get; set; } = string.Empty;
    }

    public class CommentReactionDto
    {
        public int ReactionId { get; set; }
        public int CommentId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string? UserPhoto { get; set; }
        public string ReactionType { get; set; } = string.Empty;
    }

    public class CommentReactionsCountDto
    {
        public int TotalCount { get; set; }
        public Dictionary<string, int> ByType { get; set; } = new();
    }
}