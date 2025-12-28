using PostCare.core.DTOS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PostCare.core.Interfaces
{
    public interface IUserService
    {
        Task<UserProfileDto> GetUserProfileAsync(int userId);
        Task<UserProfileDto> UpdateUserProfileAsync(int userId, UpdateUserProfileDto updateDto);
        Task<bool> DeleteUserAccountAsync(int userId);
        Task ChangePasswordAsync(int userId, ChangePasswordDto changePasswordDto);
        Task<UserResponseDto> GetUserByIdAsync(int userId);
        Task<IEnumerable<UserResponseDto>> GetAllUsersAsync();
        Task<UserResponseDto> UpdateUserAsync(int userId, UpdateUserProfileDto updateDto);
        Task<bool> DeleteUserAsync(int userId);
    }
}
