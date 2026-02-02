using PostCare.core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PostCare.core.Interfaces
{
    /// <summary>
    /// Repository interface for Sleep Reference operations
    /// </summary>
    public interface ISleepReferenceRepository
    {
        Task<SleepReference?> GetByIdAsync(int sleepRefId);
        Task<SleepReference?> GetByAgeAsync(int ageInMonths);
        Task<List<SleepReference>> GetAllAsync();
        Task<int> SaveChangesAsync();
    }
}
