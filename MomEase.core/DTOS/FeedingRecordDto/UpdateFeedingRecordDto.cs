using MomEase.core.Enums;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace MomEase.core.DTOS.FeedingRecordDto
{
    public class UpdateFeedingRecordDto
    {
        [Required(ErrorMessage = "Feeding date is required")]
        public DateTime FeedingDate { get; set; }

        [Required(ErrorMessage = "Feeding times per day is required")]
        [Range(1, 20, ErrorMessage = "Feeding times must be between 1 and 20")]
        public int FeedingTimesPerDay { get; set; }

        [Required(ErrorMessage = "Feeding type is required")]
        [JsonConverter(typeof(JsonStringEnumConverter))]  
        public FeedingTypeForBaby FeedingTypeForBaby { get; set; }

        [MaxLength(500, ErrorMessage = "Notes cannot exceed 500 characters")]
        public string? Notes { get; set; }
    }
}