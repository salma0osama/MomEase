namespace PostCare.core.DTOS.FeedingRecordDto
{
    public class WeeklyFeedingDto
    {
        public DateTime WeekStart { get; set; }
        public DateTime WeekEnd { get; set; }
        public List<DailyFeedingDto> DailyRecords { get; set; }
        public double WeeklyAverage { get; set; }
    }
}