using MomEase.core.Entities;
using MomEase.core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MomEase.infra.Repositories
{
    public interface ICryReasonsRepository
    {
        Task<CryReasons> AddAsync(CryReasons reason);
        Task<CryReasons> GetByIdAsync(int id);
        Task<CryReasons> GetByNameAsync(CryReasonName name);
        Task<List<CryReasons>> GetAllAsync();
        Task<CryReasons> UpdateAsync(CryReasons reason);
        Task DeleteAsync(int id);
    }
}
