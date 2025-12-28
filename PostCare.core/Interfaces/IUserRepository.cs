using PostCare.core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PostCare.core.Interfaces
{
    public interface IUserRepository
    {
        Task<Users> GetByIdAsync(int userId);
        Task<Users> GetByEmailAsync(string email);
        Task<IEnumerable<Users>> GetAllAsync();
        Task<Users> UpdateAsync(Users user);
        Task<bool> DeleteAsync(int userId);
        Task<bool> ExistsAsync(int userId);
        Task<int> GetChildrenCountAsync(int userId);
        Task<bool> HasMotherProfileAsync(int userId);
    }
}
