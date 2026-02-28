using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using global::MomEase.core.DTOS.VaccinationDTO;
using MomEase.core.DTOS.VaccinationDTO;

namespace MomEase.core.Interfaces
{
    public interface IVaccinationService
    {
        // VaccinationsController
        Task<List<VaccinationScheduleDto>> GetAllVaccinationsAsync();
        Task<VaccinationScheduleDto> GetVaccinationByIdAsync(int scheduleId);
        Task<List<VaccinationScheduleDto>> GetVaccinationsByAgeAsync(int ageInMonths);

        // ChildVaccinationController
        Task<ChildVaccinationDto> AddChildVaccinationAsync(int childId, int userId, int scheduleId);
        Task<List<VaccinationGroupedByAgeDto>> GetChildVaccinationsGroupedAsync(int childId, int userId);
        Task<ChildVaccinationDto> GetChildVaccinationByIdAsync(int childVaccineId, int childId, int userId);
        Task<ChildVaccinationDto> UpdateVaccinationStatusAsync(int childVaccineId, int childId, int userId, UpdateVaccinationStatusDto dto);
        Task DeleteChildVaccinationAsync(int childVaccineId, int childId, int userId);
        Task<List<ChildVaccinationDto>> GetUpcomingVaccinationsAsync(int childId, int userId, int daysAhead = 30);
        Task<List<ChildVaccinationDto>> GetOverdueVaccinationsAsync(int childId, int userId);
        Task<List<ChildVaccinationDto>> GetCompletedVaccinationsAsync(int childId, int userId);
        Task<ChildVaccinationDto> MarkVaccinationAsTakenAsync(int childVaccineId, int childId, int userId, UpdateVaccinationStatusDto dto);

        // Auto-assign لما يتسجل طفل جديد
        Task AssignVaccinationsToChildAsync(int childId, DateTime birthDate);
    }
}