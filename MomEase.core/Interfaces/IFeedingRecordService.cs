using MomEase.core.DTOS.FeedingRecordDto;

namespace MomEase.core.Interfaces
{
    /// <summary>
    /// Service interface for Feeding Record operations
    /// </summary>
    public interface IFeedingRecordService
    {
        Task<FeedingRecordResponseDto> CreateFeedingRecordAsync(int childId, CreateFeedingRecordDto createDto);
        Task<List<FeedingRecordResponseDto>> GetAllRecordsForChildAsync(int childId);
        Task<FeedingRecordResponseDto> GetRecordByIdAsync(int recordId);
        Task<FeedingRecordResponseDto> UpdateFeedingRecordAsync(int recordId, UpdateFeedingRecordDto updateDto);
        Task<bool> DeleteFeedingRecordAsync(int recordId);
        Task<FeedingStatisticsDto> GetFeedingStatisticsAsync(int childId);
        Task<WeeklyFeedingDto> GetWeeklyRecordsAsync(int childId);
        Task<MonthlyFeedingDto> GetMonthlyRecordsAsync(int childId);
    }
}