using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using MomEase.core.Entities;


namespace MomEase.core.Interfaces
{
    public interface IVaccinationRepository
    {
        // Vaccinations Table
        Task<List<Vaccinations>> GetAllVaccinationSchedulesAsync();
        Task<Vaccinations?> GetVaccinationByIdAsync(int scheduleId);
        Task<List<Vaccinations>> GetVaccinationsByAgeAsync(int ageInMonths);

        // ChildVaccination Table
        Task<List<ChildVaccination>> GetChildVaccinationsAsync(int childId);
        Task<ChildVaccination?> GetChildVaccinationByIdAsync(int childVaccineId, int childId);
        Task AddChildVaccinationsAsync(List<ChildVaccination> vaccinations);
        Task<ChildVaccination> AddSingleChildVaccinationAsync(ChildVaccination vaccination);
        Task UpdateChildVaccinationAsync(ChildVaccination vaccination);
        Task DeleteChildVaccinationAsync(ChildVaccination vaccination);
        Task<List<ChildVaccination>> GetUpcomingVaccinationsAsync(int childId, int daysAhead);
        Task<List<ChildVaccination>> GetOverdueVaccinationsAsync(int childId);
        Task<List<ChildVaccination>> GetCompletedVaccinationsAsync(int childId);
        Task<bool> CheckChildOwnershipAsync(int childId, int userId);
    }
}