namespace MomEase.core.DTOS.FeedingRecordDto
{
    public class DailyFeedingDto
    {
        public DateTime Date { get; set; }
        public List<DailyFeedingEntryDto> Records { get; set; } = new();
    }

    public class DailyFeedingEntryDto
    {
        public int? TimesPerDay { get; set; }
        public string? FeedingType { get; set; }
        public string? Status { get; set; }
    }
}