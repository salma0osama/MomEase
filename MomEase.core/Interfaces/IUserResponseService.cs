using MomEase.core.DTOS;
using MomEase.core.DTOS.AssessmentDto;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MomEase.core.Interfaces
{
    public interface IUserResponseService
    {
        Task<IEnumerable<UserResponseForAssessmentDto>> GetResponsesByResultIdAsync(int userId, int resultId);
        Task<UserResponseForAssessmentDto?> GetResponseByIdAsync(int userId, int resultId, int responseId);
    }
}