namespace PostCare.core.DTOS.FeedingRecordDto
{
    public class FeedingRecordResponseDto
    {
        public int RecordId { get; set; }
        public int ChildId { get; set; }
        public string ChildName { get; set; }
        public DateTime FeedingDate { get; set; }
        public int FeedingTimesPerDay { get; set; }
        public string FeedingTypeForBaby { get; set; } // نوع الأكل (Breastfeeding, Formula, SolidFood)
        public string FeedingType { get; set; }        // الحالة (Normal, Under, Over, SevereUnder, Obese)
        public string? Notes { get; set; }
        public FeedingReferenceInfo? ReferenceInfo { get; set; }
    }
}