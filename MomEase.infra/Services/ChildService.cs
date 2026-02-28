using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using MomEase.core.DTOS.ChildDTO;
using MomEase.core.Entities;
using MomEase.core.Enums;
using MomEase.core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MomEase.infra.Services
{
    public class ChildService : IChildService
    {
        private readonly IChildRepository _childRepository;
        private readonly IFileStorageService _fileStorageService;
        private readonly IVaccinationService _vaccinationService;
        private readonly ILogger<ChildService> _logger;

        public ChildService(
            IChildRepository childRepository,
            IFileStorageService fileStorageService,
            IVaccinationService vaccinationService,
            ILogger<ChildService> logger)
        {
            _childRepository = childRepository;
            _fileStorageService = fileStorageService;
            _vaccinationService = vaccinationService;
            _logger = logger;
        }

        // 1. POST /api/children - Add a new child
        public async Task<ChildDto> CreateChildAsync(int userId, CreateChildDto dto)
        {
            // Validate data
            if (string.IsNullOrWhiteSpace(dto.FullName))
                throw new ArgumentException("Child name is required");

            if (dto.BirthDate > DateTime.Now)
                throw new ArgumentException("Birth date cannot be in the future");

            if (dto.BirthDate < DateTime.Now.AddYears(-5))
                throw new ArgumentException("Birth date must be within the last five years");

            // Parse enums (case-insensitive)
            if (!Enum.TryParse<Gender>(dto.Gender, true, out var gender))
                throw new ArgumentException("Invalid gender. Must be Boy or Girl");

            if (!Enum.TryParse<DeliveryType>(dto.DeliveryType, true, out var deliveryType))
                throw new ArgumentException("Invalid delivery type. Must be Normal or Cesarean");

            if (string.IsNullOrEmpty(dto.FeedingTypeForBaby))
                throw new ArgumentException("Feeding type is required");
            if (!Enum.TryParse<FeedingTypeForBaby>(dto.FeedingTypeForBaby, true, out var feedingType))
                throw new ArgumentException("Invalid feeding type. Must be Breastfeeding, Formula, or SolidFood");

            // Create child entity
            var child = new Child
            {
                UserId = userId,
                FullName = dto.FullName.Trim(),
                Gender = gender,
                BirthDate = dto.BirthDate,
                DeliveryType = deliveryType,
                FeedingTypeForBaby = feedingType
            };

            // Save to database
            var createdChild = await _childRepository.AddChildAsync(child);
            
            await _vaccinationService.AssignVaccinationsToChildAsync(
            createdChild.ChildId, createdChild.BirthDate);

            _logger.LogInformation("New child created {ChildId} for user {UserId}",
                createdChild.ChildId, userId);

            return MapToDto(createdChild);

        }

        // 2. GET /api/children - Get all children of current user
        public async Task<List<ChildDto>> GetUserChildrenAsync(int userId)
        {
            var children = await _childRepository.GetUserChildrenAsync(userId);
            return children.Select(c => MapToDto(c)).ToList();
        }

        // 3. GET /api/children/{id} - Get a specific child by ID
        public async Task<ChildDto> GetChildByIdAsync(int childId, int userId)
        {
            var child = await _childRepository.GetChildByIdAsync(childId);

            if (child == null)
                throw new KeyNotFoundException("Child not found");

            if (child.UserId != userId)
                throw new UnauthorizedAccessException("You are not authorized to access this child's data");

            return MapToDto(child);
        }

        // 4. PUT /api/children/{id} - Update child data
        public async Task<ChildDto> UpdateChildAsync(int childId, int userId, UpdateChildDto dto)
        {
            var child = await _childRepository.GetChildByIdAsync(childId);

            if (child == null)
                throw new KeyNotFoundException("Child not found");

            if (child.UserId != userId)
                throw new UnauthorizedAccessException("You are not authorized to update this child's data");

            // Update fields
            if (!string.IsNullOrWhiteSpace(dto.FullName))
                child.FullName = dto.FullName.Trim();

            if (!string.IsNullOrEmpty(dto.Gender) && Enum.TryParse<Gender>(dto.Gender, true, out var gender))
                child.Gender = gender;

            if (dto.BirthDate.HasValue && dto.BirthDate.Value <= DateTime.Now && dto.BirthDate.Value > DateTime.Now.AddYears(-5))
                child.BirthDate = dto.BirthDate.Value;

            if (!string.IsNullOrEmpty(dto.DeliveryType) && Enum.TryParse<DeliveryType>(dto.DeliveryType, true, out var deliveryType))
                child.DeliveryType = deliveryType;

            if (!string.IsNullOrEmpty(dto.FeedingTypeForBaby) && Enum.TryParse<FeedingTypeForBaby>(dto.FeedingTypeForBaby, true, out var feedingType))
                child.FeedingTypeForBaby = feedingType;

            // Save updates
            var updatedChild = await _childRepository.UpdateChildAsync(child);

            _logger.LogInformation("Child data updated {ChildId}", childId);

            return MapToDto(updatedChild);
        }

        // 5. DELETE /api/children/{id} - Delete a child
        public async Task<bool> DeleteChildAsync(int childId, int userId)
        {
            var child = await _childRepository.GetChildByIdAsync(childId);

            if (child == null)
                throw new KeyNotFoundException("Child not found");

            if (child.UserId != userId)
                throw new UnauthorizedAccessException("You are not authorized to delete this child");

            // Delete photo if exists
            if (!string.IsNullOrEmpty(child.PhotoUrl))
            {
                await _fileStorageService.DeleteFileAsync(child.PhotoUrl);
            }

            // Delete child from database
            await _childRepository.DeleteChildAsync(child);

            _logger.LogInformation("Child deleted {ChildId}", childId);

            return true;
        }

        // 6. POST /api/children/{id}/photo - Upload a child photo
        public async Task<string> UploadChildPhotoAsync(int childId, int userId, IFormFile photo)
        {
            var child = await _childRepository.GetChildByIdAsync(childId);

            if (child == null)
                throw new KeyNotFoundException("Child not found");

            if (child.UserId != userId)
                throw new UnauthorizedAccessException("You are not authorized to upload a photo for this child");

            // Validate file
            if (photo == null || photo.Length == 0)
                throw new ArgumentException("Invalid file");

            // Check file size (max 5 MB)
            if (photo.Length > 5 * 1024 * 1024)
                throw new ArgumentException("Photo size must be less than 5 MB");

            // Check file type
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
            var extension = Path.GetExtension(photo.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension))
                throw new ArgumentException("Unsupported file type. Allowed types: JPG, JPEG, PNG");

            // Delete old photo if exists
            if (!string.IsNullOrEmpty(child.PhotoUrl))
            {
                await _fileStorageService.DeleteFileAsync(child.PhotoUrl);
            }

            // Upload new photo
            var photoUrl = await _fileStorageService.UploadFileAsync(photo, "children");

            // Update PhotoUrl in database
            await _childRepository.UpdateChildPhotoAsync(childId, photoUrl);

            _logger.LogInformation("Photo uploaded for child {ChildId}", childId);

            return photoUrl;
        }

        // 7. DELETE /api/children/{id}/photo - Delete a child photo
        public async Task<bool> DeleteChildPhotoAsync(int childId, int userId)
        {
            var child = await _childRepository.GetChildByIdAsync(childId);

            if (child == null)
                throw new KeyNotFoundException("Child not found");

            if (child.UserId != userId)
                throw new UnauthorizedAccessException("You are not authorized to delete this child's photo");

            if (string.IsNullOrEmpty(child.PhotoUrl))
                throw new InvalidOperationException("No photo to delete");

            // Delete file from server
            await _fileStorageService.DeleteFileAsync(child.PhotoUrl);

            // Update database
            await _childRepository.DeleteChildPhotoAsync(childId);

            _logger.LogInformation("Child photo deleted {ChildId}", childId);

            return true;
        }

        // Helper: Map Entity to DTO
        private ChildDto MapToDto(Child child)
        {
            var ageInMonths = CalculateAgeInMonths(child.BirthDate);
            var ageInDays = CalculateAgeInDays(child.BirthDate);

            return new ChildDto
            {
                ChildId = child.ChildId,
                FullName = child.FullName,
                Gender = child.Gender.ToString(),
                BirthDate = child.BirthDate,
                AgeInMonths = ageInMonths,
                AgeInDays = ageInDays,
                DeliveryType = child.DeliveryType.ToString(),
                FeedingTypeForBaby = child.FeedingTypeForBaby.ToString(),
                PhotoUrl = child.PhotoUrl
            };
        }

        // Helper: Calculate age in months
        private int CalculateAgeInMonths(DateTime birthDate)
        {
            var today = DateTime.Now;
            var months = ((today.Year - birthDate.Year) * 12) + today.Month - birthDate.Month;
            if (today.Day < birthDate.Day)
                months--;
            return months < 0 ? 0 : months;
        }

        // Helper: Calculate age in days
        private int CalculateAgeInDays(DateTime birthDate)
        {
            var today = DateTime.Now;
            var days = (today - birthDate).Days;
            return days < 0 ? 0 : days;
        }
    }
}
