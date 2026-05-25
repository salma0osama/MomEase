namespace MomEase.core.DTOS.FeedingRecordDto
{
    public class MonthlyFeedingDto
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public string MonthName { get; set; }
        public List<DailyFeedingDto> DailyRecords { get; set; }  
        public double MonthlyAverageTimesPerDay { get; set; }    
        public int TotalRecords { get; set; }
        public int NormalDays { get; set; }                    
        public int AbnormalDays { get; set; }
    }
}