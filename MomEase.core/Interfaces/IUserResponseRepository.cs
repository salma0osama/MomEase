using MomEase.core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MomEase.core.Interfaces
{
    public interface IUserResponseRepository
    {
        Task<IEnumerable<UserResponse>> GetByResultIdAsync(int resultId);
        Task<UserResponse?> GetByIdAsync(int resultId, int responseId);
        Task<UserResponse> CreateAsync(UserResponse response);
    }
}