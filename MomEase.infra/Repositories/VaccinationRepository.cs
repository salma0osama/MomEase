using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MomEase.core.Entities;
using MomEase.core.Enums;
using MomEase.core.Interfaces;
using MomEase.infra.Data;

namespace MomEase.infra.Repositories
{
    public class VaccinationRepository : IVaccinationRepository
    {
        private readonly MomEaseDbContext _context;
        private readonly ILogger<VaccinationRepository> _logger;

        public VaccinationRepository(MomEaseDbContext context, ILogger<VaccinationRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        // ===== Vaccinations Table =====

        public async Task<List<Vaccinations>> GetAllVaccinationSchedulesAsync()
        {
            try
            {
                return await _context.Vaccinations
                    .OrderBy(v => v.Age)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all vaccination schedules");
                throw;
            }
        }

        public async Task<Vaccinations?> GetVaccinationByIdAsync(int scheduleId)
        {
            try
            {
                return await _context.Vaccinations
                    .FirstOrDefaultAsync(v => v.ScheduleId == scheduleId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving vaccination {ScheduleId}", scheduleId);
                throw;
            }
        }

        public async Task<List<Vaccinations>> GetVaccinationsByAgeAsync(int ageInMonths)
        {
            try
            {
                return await _context.Vaccinations
                    .Where(v => v.Age == ageInMonths)
                    .OrderBy(v => v.ScheduleId)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving vaccinations for age {Age}", ageInMonths);
                throw;
            }
        }

        // ===== ChildVaccination Table =====

        public async Task<List<ChildVaccination>> GetChildVaccinationsAsync(int childId)
        {
            try
            {
                return await _context.ChildVaccinations
                    .Include(cv => cv.Vaccination)
                    .Include(cv => cv.Child) // ✅ مهم للـ ValidateChildOwnership
                    .Where(cv => cv.ChildId == childId)
                    .OrderBy(cv => cv.ScheduledDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving vaccinations for child {ChildId}", childId);
                throw;
            }
        }

        public async Task<ChildVaccination?> GetChildVaccinationByIdAsync(int childVaccineId, int childId)
        {
            try
            {
                return await _context.ChildVaccinations
                    .Include(cv => cv.Vaccination)
                    .FirstOrDefaultAsync(cv => cv.ChildVaccineId == childVaccineId
                                            && cv.ChildId == childId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving vaccination {ChildVaccineId}", childVaccineId);
                throw;
            }
        }

        public async Task AddChildVaccinationsAsync(List<ChildVaccination> vaccinations)
        {
            try
            {
                await _context.ChildVaccinations.AddRangeAsync(vaccinations);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding vaccinations");
                throw;
            }
        }

        public async Task<ChildVaccination> AddSingleChildVaccinationAsync(ChildVaccination vaccination)
        {
            try
            {
                await _context.ChildVaccinations.AddAsync(vaccination);
                await _context.SaveChangesAsync();
                return vaccination;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding single vaccination");
                throw;
            }
        }

        public async Task UpdateChildVaccinationAsync(ChildVaccination vaccination)
        {
            try
            {
                _context.ChildVaccinations.Update(vaccination);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating vaccination {ChildVaccineId}", vaccination.ChildVaccineId);
                throw;
            }
        }

        public async Task DeleteChildVaccinationAsync(ChildVaccination vaccination)
        {
            try
            {
                _context.ChildVaccinations.Remove(vaccination);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting vaccination {ChildVaccineId}", vaccination.ChildVaccineId);
                throw;
            }
        }

        public async Task<List<ChildVaccination>> GetUpcomingVaccinationsAsync(int childId, int daysAhead)
        {
            try
            {
                var today = DateTime.Now.Date;
                var futureDate = today.AddDays(daysAhead);

                return await _context.ChildVaccinations
                    .Include(cv => cv.Vaccination)
                    .Where(cv => cv.ChildId == childId
                              && cv.Status == VaccineStatus.Pending
                              && cv.ScheduledDate.Date >= today
                              && cv.ScheduledDate.Date <= futureDate)
                    .OrderBy(cv => cv.ScheduledDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving upcoming vaccinations for child {ChildId}", childId);
                throw;
            }
        }

        public async Task<List<ChildVaccination>> GetOverdueVaccinationsAsync(int childId)
        {
            try
            {
                var today = DateTime.Now.Date;

                return await _context.ChildVaccinations
                    .Include(cv => cv.Vaccination)
                    .Where(cv => cv.ChildId == childId
                              && cv.Status == VaccineStatus.Missed
                              && cv.ScheduledDate.Date < today)
                    .OrderBy(cv => cv.ScheduledDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving overdue vaccinations for child {ChildId}", childId);
                throw;
            }
        }

        public async Task<List<ChildVaccination>> GetCompletedVaccinationsAsync(int childId)
        {
            try
            {
                return await _context.ChildVaccinations
                    .Include(cv => cv.Vaccination)
                    .Where(cv => cv.ChildId == childId
                              && cv.Status == VaccineStatus.Done)
                    .OrderBy(cv => cv.TakenDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving completed vaccinations for child {ChildId}", childId);
                throw;
            }
        }
        public async Task<bool> CheckChildOwnershipAsync(int childId, int userId)
        {
            return await _context.Children
                .AnyAsync(c => c.ChildId == childId && c.UserId == userId);
        }
    }
}