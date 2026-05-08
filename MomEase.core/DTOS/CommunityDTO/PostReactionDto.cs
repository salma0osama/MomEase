namespace MomEase.core.DTOS.CommunityDTO
{
    public class AddReactionDto
    {
        public string ReactionType { get; set; } = string.Empty;
    }

    public class UpdateReactionDto
    {
        public string ReactionType { get; set; } = string.Empty;
    }

    public class PostReactionDto
    {
        public int ReactionId { get; set; }
        public int PostId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string ReactionType { get; set; } = string.Empty;
        public string? UserPhoto { get; set; }
    }

    public class ReactionsCountDto
    {
        public int TotalCount { get; set; }
        public Dictionary<string, int> ByType { get; set; } = new();
    }
}