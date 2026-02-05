namespace MomEase.core.DTOS.FeedingRecordDto
{
    public class FeedingStatisticsDto
    {
        public int TotalRecords { get; set; }
        public double AverageTimesPerDay { get; set; }
        public double Last7DaysAverage { get; set; }
        public string CurrentFeedingStatus { get; set; }
        public string MostCommonFeedingType { get; set; }
        public ComparisonWithReferenceDto ComparisonWithReference { get; set; }
    }
}