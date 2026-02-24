using MomEase.core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MomEase.core.Interfaces
{
    public interface ISearchHistoryRepository
    {
        Task<SearchHistory> CreateAsync(SearchHistory searchHistory);
        Task<IEnumerable<SearchHistory>> GetByUserIdAsync(int userId);
        Task<bool> DeleteAllByUserIdAsync(int userId);
        Task<bool> DeleteBySearchTermAsync(int userId, string searchTerm);
        Task<bool> ExistsAsync(int userId, string searchTerm);
    }
}
