namespace PostCare.core.DTOS.FeedingRecordDto
{
    public class MonthlyFeedingDto
    {
        public int Month { get; set; }
        public int Year { get; set; }
        public string MonthName { get; set; }
        public int TotalRecords { get; set; }
        public double AverageTimesPerDay { get; set; }
        public Dictionary<string, int> FeedingTypeDistribution { get; set; }
        public string DominantStatus { get; set; }
    }
}