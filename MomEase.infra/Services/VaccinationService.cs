using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using MomEase.core.DTOS.VaccinationDTO;
using MomEase.core.Entities;
using MomEase.core.Enums;
using MomEase.core.Interfaces;
using MomEase.infra.Helpers;

namespace MomEase.infra.Services
{
    public class VaccinationService : IVaccinationService
    {
        private readonly IVaccinationRepository _vaccinationRepository;
        private readonly INotificationService _notificationService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<VaccinationService> _logger;

        public VaccinationService(
            IVaccinationRepository vaccinationRepository,
            INotificationService notificationService,
            IHttpContextAccessor httpContextAccessor,
            ILogger<VaccinationService> logger)
        {
            _vaccinationRepository = vaccinationRepository;
            _notificationService = notificationService;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public async Task<List<VaccinationScheduleDto>> GetAllVaccinationsAsync()
        {
            try
            {
                var vaccinations = await _vaccinationRepository.GetAllVaccinationSchedulesAsync();
                return vaccinations.Select(v => MapToScheduleDto(v)).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all vaccinations");
                throw;
            }
        }

        public async Task<VaccinationScheduleDto> GetVaccinationByIdAsync(int scheduleId)
        {
            try
            {
                var vaccination = await _vaccinationRepository.GetVaccinationByIdAsync(scheduleId);
                if (vaccination == null)
                    throw new KeyNotFoundException("Vaccination not found");
                return MapToScheduleDto(vaccination);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving vaccination {ScheduleId}", scheduleId);
                throw;
            }
        }

        public async Task<List<VaccinationScheduleDto>> GetVaccinationsByAgeAsync(int ageInMonths)
        {
            try
            {
                var vaccinations = await _vaccinationRepository.GetVaccinationsByAgeAsync(ageInMonths);
                return vaccinations.Select(v => MapToScheduleDto(v)).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving vaccinations for age {Age}", ageInMonths);
                throw;
            }
        }

        public async Task<ChildVaccinationDto> AddChildVaccinationAsync(
            int childId, int userId, int scheduleId)
        {
            try
            {
                await ValidateChildOwnershipAsync(childId, userId);

                var vaccination = await _vaccinationRepository.GetVaccinationByIdAsync(scheduleId);
                if (vaccination == null)
                    throw new KeyNotFoundException("Vaccination schedule not found");

                var existingVaccinations = await _vaccinationRepository
                    .GetChildVaccinationsAsync(childId);

                if (!existingVaccinations.Any())
                    throw new KeyNotFoundException("Child has no vaccination records");

                var firstRecord = existingVaccinations.OrderBy(v => v.ScheduledDate).First();
                var birthDate = firstRecord.ScheduledDate.AddMonths(-firstRecord.Vaccination.Age);

                var childVaccination = new ChildVaccination
                {
                    ChildId = childId,
                    ScheduleId = scheduleId,
                    ScheduledDate = birthDate.AddMonths(vaccination.Age),
                    Status = VaccineStatus.Pending
                };

                var result = await _vaccinationRepository
                    .AddSingleChildVaccinationAsync(childVaccination);

                var added = await _vaccinationRepository
                    .GetChildVaccinationByIdAsync(result.ChildVaccineId, childId);

                return MapToChildVaccinationDto(added!);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding vaccination to child {ChildId}", childId);
                throw;
            }
        }

        public async Task<List<VaccinationGroupedByAgeDto>> GetChildVaccinationsGroupedAsync(
            int childId, int userId)
        {
            try
            {
                await ValidateChildOwnershipAsync(childId, userId);
                await AutoUpdateMissedVaccinationsAsync(childId);

                var vaccinations = await _vaccinationRepository.GetChildVaccinationsAsync(childId);

                var grouped = vaccinations
                    .GroupBy(cv => cv.Vaccination.Age)
                    .OrderBy(g => g.Key)
                    .Select(g => new VaccinationGroupedByAgeDto
                    {
                        AgeInMonths = g.Key,
                        AgeLabel = GetAgeLabel(g.Key),
                        ScheduledDate = g.First().ScheduledDate,
                        Vaccines = g.Select(cv => MapToChildVaccinationDto(cv)).ToList()
                    })
                    .ToList();

                return grouped;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving grouped vaccinations for child {ChildId}", childId);
                throw;
            }
        }

        public async Task<ChildVaccinationDto> GetChildVaccinationByIdAsync(
            int childVaccineId, int childId, int userId)
        {
            try
            {
                await ValidateChildOwnershipAsync(childId, userId);

                var vaccination = await _vaccinationRepository
                    .GetChildVaccinationByIdAsync(childVaccineId, childId);

                if (vaccination == null)
                    throw new KeyNotFoundException("Vaccination record not found");

                return MapToChildVaccinationDto(vaccination);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving vaccination {ChildVaccineId}", childVaccineId);
                throw;
            }
        }

        public async Task<ChildVaccinationDto> UpdateVaccinationStatusAsync(
            int childVaccineId, int childId, int userId, UpdateVaccinationStatusDto dto)
        {
            try
            {
                await ValidateChildOwnershipAsync(childId, userId);

                var vaccination = await _vaccinationRepository
                    .GetChildVaccinationByIdAsync(childVaccineId, childId);

                if (vaccination == null)
                    throw new KeyNotFoundException("Vaccination record not found");

                if (!Enum.TryParse<VaccineStatus>(dto.Status, true, out var newStatus))
                    throw new ArgumentException("Invalid status. Must be: Pending, Done, or Missed");

                vaccination.Status = newStatus;
                vaccination.TakenDate = dto.TakenDate;

                await _vaccinationRepository.UpdateChildVaccinationAsync(vaccination);

                return MapToChildVaccinationDto(vaccination);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating vaccination status {ChildVaccineId}", childVaccineId);
                throw;
            }
        }

        public async Task DeleteChildVaccinationAsync(
            int childVaccineId, int childId, int userId)
        {
            try
            {
                await ValidateChildOwnershipAsync(childId, userId);

                var vaccination = await _vaccinationRepository
                    .GetChildVaccinationByIdAsync(childVaccineId, childId);

                if (vaccination == null)
                    throw new KeyNotFoundException("Vaccination record not found");

                await _vaccinationRepository.DeleteChildVaccinationAsync(vaccination);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting vaccination {ChildVaccineId}", childVaccineId);
                throw;
            }
        }

        public async Task<List<ChildVaccinationDto>> GetUpcomingVaccinationsAsync(
            int childId, int userId, int daysAhead = 30)
        {
            try
            {
                await ValidateChildOwnershipAsync(childId, userId);
                await AutoUpdateMissedVaccinationsAsync(childId);

                var upcoming = await _vaccinationRepository
                    .GetUpcomingVaccinationsAsync(childId, daysAhead);

                var soonVaccinations = upcoming.Where(v =>
                    (v.ScheduledDate - DateTime.Now).TotalDays <= 7
                    && (v.ScheduledDate - DateTime.Now).TotalDays >= 0)
                    .ToList();

                foreach (var vaccination in soonVaccinations)
                {
                    await _notificationService.SendRealtimeNotificationAsync(
                        userId,
                        "Vaccination Reminder",
                        $"{vaccination.Vaccination.Vaccine} is due on {vaccination.ScheduledDate:dd/MM/yyyy}",
                        "VaccinationUpcoming",
                        vaccination.ChildVaccineId);
                }

                return upcoming.Select(cv => MapToChildVaccinationDto(cv)).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving upcoming vaccinations for child {ChildId}", childId);
                throw;
            }
        }

        public async Task<List<ChildVaccinationDto>> GetOverdueVaccinationsAsync(
            int childId, int userId)
        {
            try
            {
                await ValidateChildOwnershipAsync(childId, userId);
                await AutoUpdateMissedVaccinationsAsync(childId);

                var overdue = await _vaccinationRepository.GetOverdueVaccinationsAsync(childId);
                return overdue.Select(cv => MapToChildVaccinationDto(cv)).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving overdue vaccinations for child {ChildId}", childId);
                throw;
            }
        }

        public async Task<List<ChildVaccinationDto>> GetCompletedVaccinationsAsync(
            int childId, int userId)
        {
            try
            {
                await ValidateChildOwnershipAsync(childId, userId);

                var completed = await _vaccinationRepository.GetCompletedVaccinationsAsync(childId);
                return completed.Select(cv => MapToChildVaccinationDto(cv)).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving completed vaccinations for child {ChildId}", childId);
                throw;
            }
        }

        public async Task<ChildVaccinationDto> MarkVaccinationAsTakenAsync(
            int childVaccineId, int childId, int userId, UpdateVaccinationStatusDto dto)
        {
            try
            {
                await ValidateChildOwnershipAsync(childId, userId);

                var vaccination = await _vaccinationRepository
                    .GetChildVaccinationByIdAsync(childVaccineId, childId);

                if (vaccination == null)
                    throw new KeyNotFoundException("Vaccination record not found");

                if (vaccination.Status == VaccineStatus.Done)
                    throw new InvalidOperationException("Vaccination already marked as done");

                vaccination.Status = VaccineStatus.Done;
                vaccination.TakenDate = dto.TakenDate ?? DateTime.Now;

                await _vaccinationRepository.UpdateChildVaccinationAsync(vaccination);

                await _notificationService.SendRealtimeNotificationAsync(
                    userId,
                    "Vaccination Completed",
                    $"{vaccination.Vaccination.Vaccine} has been successfully completed!",
                    "VaccinationCompleted",
                    vaccination.ChildVaccineId);

                return MapToChildVaccinationDto(vaccination);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking vaccination as taken {ChildVaccineId}", childVaccineId);
                throw;
            }
        }

        public async Task AssignVaccinationsToChildAsync(int childId, DateTime birthDate)
        {
            try
            {
                var allVaccinations = await _vaccinationRepository.GetAllVaccinationSchedulesAsync();

                var childVaccinations = allVaccinations.Select(v => new ChildVaccination
                {
                    ChildId = childId,
                    ScheduleId = v.ScheduleId,
                    ScheduledDate = birthDate.AddMonths(v.Age),
                    Status = VaccineStatus.Pending
                }).ToList();

                await _vaccinationRepository.AddChildVaccinationsAsync(childVaccinations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning vaccinations to child {ChildId}", childId);
                throw;
            }
        }

        // ===== Private Helpers =====

        private async Task ValidateChildOwnershipAsync(int childId, int userId)
        {
            var isOwner = await _vaccinationRepository.CheckChildOwnershipAsync(childId, userId);
            if (!isOwner)
                throw new KeyNotFoundException("Child not found");
        }

        private async Task AutoUpdateMissedVaccinationsAsync(int childId)
        {
            var allVaccinations = await _vaccinationRepository.GetChildVaccinationsAsync(childId);
            var today = DateTime.Now.Date;

            var missedVaccinations = allVaccinations
                .Where(cv => cv.Status == VaccineStatus.Pending
                          && cv.ScheduledDate.Date < today)
                .ToList();

            foreach (var vaccination in missedVaccinations)
            {
                vaccination.Status = VaccineStatus.Missed;
                await _vaccinationRepository.UpdateChildVaccinationAsync(vaccination);
            }
        }

        private string GetAgeLabel(int ageInMonths)
        {
            var lang = LanguageHelper.GetLang(_httpContextAccessor);
            if (lang.StartsWith("ar"))
            {
                return ageInMonths switch
                {
                    0 => "عند الولادة",
                    1 => "شهر واحد",
                    _ => $"{ageInMonths} أشهر"
                };
            }
            return ageInMonths switch
            {
                0 => "At Birth",
                1 => "1 Month",
                _ => $"{ageInMonths} Months"
            };
        }

        private VaccinationScheduleDto MapToScheduleDto(Vaccinations v)
        {
            var lang = LanguageHelper.GetLang(_httpContextAccessor);
            return new VaccinationScheduleDto
            {
                ScheduleId = v.ScheduleId,
                Vaccine = LanguageHelper.GetLocalized(v.VaccineAr, v.Vaccine, lang),
                AgeInMonths = v.Age,
                DoseTiming = LanguageHelper.GetLocalized(v.DoseTimingAr, v.DoseTiming, lang),
                DiseasePrevented = LanguageHelper.GetLocalized(v.DiseasePreventedAr, v.DiseasePrevented, lang),
                Dosage = LanguageHelper.GetLocalized(v.DosageAr, v.Dosage, lang),
                VaccinationWay = LanguageHelper.GetLocalized(v.VaccinationWayAr, v.VaccinationWay, lang)
            };
        }

        private ChildVaccinationDto MapToChildVaccinationDto(ChildVaccination cv)
        {
            var lang = LanguageHelper.GetLang(_httpContextAccessor);
            return new ChildVaccinationDto
            {
                ChildVaccineId = cv.ChildVaccineId,
                ChildId = cv.ChildId,
                ScheduleId = cv.ScheduleId,
                VaccineName = LanguageHelper.GetLocalized(
                    cv.Vaccination?.VaccineAr, cv.Vaccination?.Vaccine ?? "", lang),
                DoseTiming = LanguageHelper.GetLocalized(
                    cv.Vaccination?.DoseTimingAr, cv.Vaccination?.DoseTiming ?? "", lang),
                DiseasePrevented = LanguageHelper.GetLocalized(
                    cv.Vaccination?.DiseasePreventedAr, cv.Vaccination?.DiseasePrevented ?? "", lang),
                Dosage = LanguageHelper.GetLocalized(
                    cv.Vaccination?.DosageAr, cv.Vaccination?.Dosage ?? "", lang),
                VaccinationWay = LanguageHelper.GetLocalized(
                    cv.Vaccination?.VaccinationWayAr, cv.Vaccination?.VaccinationWay ?? "", lang),
                AgeInMonths = cv.Vaccination?.Age ?? 0,
                ScheduledDate = cv.ScheduledDate,
                TakenDate = cv.TakenDate,
                Status = cv.Status.ToString()
            };
        }
    }
}