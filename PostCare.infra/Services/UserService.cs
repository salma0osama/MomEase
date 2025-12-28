using PostCare.core.DTOS;
using PostCare.core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PostCare.infra.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<UserProfileDto> GetUserProfileAsync(int userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);

            if (user == null)
                throw new Exception("User not found");

            return new UserProfileDto
            {
                UserId = user.UserId,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Phone = user.Phone,
                Age = user.Age,
                Role = user.Role.ToString(),
                CreatedAt = user.CreatedAt
            };
        }

        public async Task<UserProfileDto> UpdateUserProfileAsync(int userId, UpdateUserProfileDto updateDto)
        {
            var user = await _userRepository.GetByIdAsync(userId);

            if (user == null)
                throw new Exception("User not found");

            if (!string.IsNullOrWhiteSpace(updateDto.FirstName) && updateDto.FirstName != "string")
                user.FirstName = updateDto.FirstName;

            if (!string.IsNullOrWhiteSpace(updateDto.LastName) && updateDto.LastName != "string")
                user.LastName = updateDto.LastName;

            if (!string.IsNullOrWhiteSpace(updateDto.Phone) && updateDto.Phone != "string")
                user.Phone = updateDto.Phone;

            if (updateDto.Age.HasValue && updateDto.Age.Value > 0)
                user.Age = updateDto.Age;

            await _userRepository.UpdateAsync(user);

            return await GetUserProfileAsync(userId);
        }

        public async Task<bool> DeleteUserAccountAsync(int userId)
        {
            var exists = await _userRepository.ExistsAsync(userId);

            if (!exists)
                throw new Exception("User not found");

            return await _userRepository.DeleteAsync(userId);
        }
        public async Task ChangePasswordAsync(int userId, ChangePasswordDto changePasswordDto)
        {
            var user = await _userRepository.GetByIdAsync(userId);

            if (user == null)
                throw new Exception("User not found");

            bool isCurrentPasswordValid = BCrypt.Net.BCrypt.Verify(
                changePasswordDto.CurrentPassword,
                user.Password
            );

            if (!isCurrentPasswordValid)
                throw new Exception("Current password is incorrect");

            if (changePasswordDto.CurrentPassword == changePasswordDto.NewPassword)
                throw new Exception("New password must be different from current password");

            var newPasswordHash = BCrypt.Net.BCrypt.HashPassword(changePasswordDto.NewPassword);

            user.Password = newPasswordHash;

            await _userRepository.UpdateAsync(user);
        }
        public async Task<UserResponseDto> GetUserByIdAsync(int userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);

            if (user == null)
                throw new Exception("User not found");

            return new UserResponseDto
            {
                UserId = user.UserId,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Phone = user.Phone,
                Age = user.Age,
                Role = user.Role.ToString(),
                CreatedAt = user.CreatedAt,
                ChildrenCount = await _userRepository.GetChildrenCountAsync(userId),
                HasMotherProfile = await _userRepository.HasMotherProfileAsync(userId)
            };
        }

        public async Task<IEnumerable<UserResponseDto>> GetAllUsersAsync()
        {
            var users = await _userRepository.GetAllAsync();
            var filteredUsers = users.Where(u => u.Role.ToString() != "Admin" && u.Role.ToString() != "ADMIN");
            var userDtos = new List<UserResponseDto>();

            foreach (var user in filteredUsers)
            {
                userDtos.Add(new UserResponseDto
                {
                    UserId = user.UserId,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    Phone = user.Phone,
                    Age = user.Age,
                    Role = user.Role.ToString(),
                    CreatedAt = user.CreatedAt,
                    ChildrenCount = user.Children?.Count ?? 0,
                    HasMotherProfile = user.MotherProfile != null
                });
            }

            return userDtos;
        }

        public async Task<UserResponseDto> UpdateUserAsync(int userId, UpdateUserProfileDto updateDto)
        {
            await UpdateUserProfileAsync(userId, updateDto);

            return await GetUserByIdAsync(userId);
        }

        public async Task<bool> DeleteUserAsync(int userId)
        {
            return await DeleteUserAccountAsync(userId);
        }
    }
}
