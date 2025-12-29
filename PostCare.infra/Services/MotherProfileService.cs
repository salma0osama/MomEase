using PostCare.core.DTOS.MotherProfileDto;
using PostCare.core.Entities;
using PostCare.core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PostCare.infra.Services
{
    /// <summary>
    /// Service implementation for Mother Profile business logic
    /// </summary>
    public class MotherProfileService : IMotherProfileService
    {
        private readonly IMotherProfileRepository _motherProfileRepository;
        private readonly IAuthRepository _authRepository;

        public MotherProfileService(
            IMotherProfileRepository motherProfileRepository,
            IAuthRepository authRepository)
        {
            _motherProfileRepository = motherProfileRepository ?? throw new ArgumentNullException(nameof(motherProfileRepository));
            _authRepository = authRepository ?? throw new ArgumentNullException(nameof(authRepository));
        }

        public async Task<MotherProfileResponseDto> CreateMotherProfileAsync(CreateMotherProfileDto createDto)
        {
            try
            {
                // Validate user exists
                var user = await _authRepository.GetUserByIdAsync(createDto.UserId);
                if (user == null)
                    throw new InvalidOperationException($"User with ID {createDto.UserId} not found");

                // Check if profile already exists
                if (await _motherProfileRepository.ExistsByUserIdAsync(createDto.UserId))
                    throw new InvalidOperationException("Mother profile already exists for this user");

                // Create mother profile
                var motherProfile = new MotherProfile
                {
                    UserId = createDto.UserId,
                    IsFirstTimeMother = createDto.IsFirstTimeMother,
                    NumberOfChildren = createDto.NumberOfChildren,
                    MentalHealthStatus = createDto.MentalHealthStatus,
                    HealthStatus = createDto.HealthStatus
                };

                await _motherProfileRepository.AddAsync(motherProfile);
                await _motherProfileRepository.SaveChangesAsync();

                // Return response
                return new MotherProfileResponseDto
                {
                    MotherId = motherProfile.MotherId,
                    UserId = user.UserId,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    IsFirstTimeMother = motherProfile.IsFirstTimeMother,
                    NumberOfChildren = motherProfile.NumberOfChildren,
                    MentalHealthStatus = motherProfile.MentalHealthStatus?.ToString(),
                    HealthStatus = motherProfile.HealthStatus?.ToString(),
                    CreatedAt = user.CreatedAt
                };
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to create mother profile: {ex.Message}", ex);
            }
        }

        public async Task<MotherProfileResponseDto> GetMyProfileAsync(int userId)
        {
            try
            {
                // Get profile with user data
                var motherProfile = await _motherProfileRepository.GetByUserIdWithUserAsync(userId);

                if (motherProfile == null)
                    throw new InvalidOperationException("Mother profile not found");

                return new MotherProfileResponseDto
                {
                    MotherId = motherProfile.MotherId,
                    UserId = motherProfile.UserId,
                    FirstName = motherProfile.User.FirstName,
                    LastName = motherProfile.User.LastName,
                    Email = motherProfile.User.Email,
                    IsFirstTimeMother = motherProfile.IsFirstTimeMother,
                    NumberOfChildren = motherProfile.NumberOfChildren,
                    MentalHealthStatus = motherProfile.MentalHealthStatus?.ToString(),
                    HealthStatus = motherProfile.HealthStatus?.ToString(),
                    CreatedAt = motherProfile.User.CreatedAt
                };
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to retrieve mother profile: {ex.Message}", ex);
            }
        }

        public async Task<MotherProfileResponseDto> GetMotherProfileByIdAsync(int motherId)
        {
            try
            {
                var motherProfile = await _motherProfileRepository.GetByIdAsync(motherId);

                if (motherProfile == null)
                    throw new InvalidOperationException($"Mother profile with ID {motherId} not found");

                // Get user data
                var user = await _authRepository.GetUserByIdAsync(motherProfile.UserId);
                if (user == null)
                    throw new InvalidOperationException("User not found for this mother profile");

                // IMPORTANT: Verify this is actually a MOTHER's profile, not ADMIN
                if (user.Role != core.Enums.Role.MOTHER)
                    throw new InvalidOperationException("This user is not a mother");

                return new MotherProfileResponseDto
                {
                    MotherId = motherProfile.MotherId,
                    UserId = motherProfile.UserId,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    IsFirstTimeMother = motherProfile.IsFirstTimeMother,
                    NumberOfChildren = motherProfile.NumberOfChildren,
                    MentalHealthStatus = motherProfile.MentalHealthStatus?.ToString(),
                    HealthStatus = motherProfile.HealthStatus?.ToString(),
                    CreatedAt = user.CreatedAt
                };
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to retrieve mother profile: {ex.Message}", ex);
            }
        }

        public async Task<MotherProfileResponseDto> UpdateMotherProfileAsync(int userId, UpdateMotherProfileDto updateDto)
        {
            try
            {
                // Get existing profile
                var motherProfile = await _motherProfileRepository.GetByUserIdAsync(userId);

                if (motherProfile == null)
                    throw new InvalidOperationException("Mother profile not found");

                // Update fields
                motherProfile.IsFirstTimeMother = updateDto.IsFirstTimeMother;
                motherProfile.NumberOfChildren = updateDto.NumberOfChildren;
                motherProfile.MentalHealthStatus = updateDto.MentalHealthStatus;
                motherProfile.HealthStatus = updateDto.HealthStatus;

                await _motherProfileRepository.UpdateAsync(motherProfile);
                await _motherProfileRepository.SaveChangesAsync();

                // Return updated profile
                return await GetMyProfileAsync(userId);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to update mother profile: {ex.Message}", ex);
            }
        }

        public async Task<bool> DeleteMotherProfileAsync(int userId)
        {
            try
            {
                var motherProfile = await _motherProfileRepository.GetByUserIdAsync(userId);

                if (motherProfile == null)
                    throw new InvalidOperationException("Mother profile not found");

                await _motherProfileRepository.DeleteAsync(motherProfile);
                await _motherProfileRepository.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to delete mother profile: {ex.Message}", ex);
            }
        }
    }
}