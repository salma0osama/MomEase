namespace MomEase.core.DTOS.FeedingRecordDto
{
    public class ComparisonWithReferenceDto
    {
        public string Status { get; set; }
        public int RecommendedMin { get; set; }
        public int RecommendedMax { get; set; }
        public double ActualAverage { get; set; }
        public string Message { get; set; }
    }
}