using MomEase.core.DTOS.MotherProfileDto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MomEase.core.Interfaces
{
    public interface IMotherProfileService
    {
        /// <summary>
        /// Create mother profile (called automatically during registration)
        /// </summary>
        Task<MotherProfileResponseDto> CreateMotherProfileAsync(CreateMotherProfileDto createDto);

        /// <summary>
        /// Get current logged-in mother's profile
        /// </summary>
        Task<MotherProfileResponseDto> GetMyProfileAsync(int userId);

        /// <summary>
        /// Get mother profile by ID (for admin)
        /// </summary>
        Task<MotherProfileResponseDto> GetMotherProfileByIdAsync(int motherId);

        /// <summary>
        /// Update mother profile
        /// </summary>
        Task<MotherProfileResponseDto> UpdateMotherProfileAsync(int userId, UpdateMotherProfileDto updateDto);

        /// <summary>
        /// Delete mother profile
        /// </summary>
        Task<bool> DeleteMotherProfileAsync(int userId);
    }
}
