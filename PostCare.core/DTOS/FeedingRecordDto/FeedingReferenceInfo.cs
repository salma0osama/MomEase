namespace PostCare.core.DTOS.FeedingRecordDto
{
    /// <summary>
    /// Reference information included in responses
    /// </summary>
    public class FeedingReferenceInfo
    {
        public int MinTimesPerDay { get; set; }
        public int MaxTimesPerDay { get; set; }
        public string AgeRange { get; set; }
    }
}