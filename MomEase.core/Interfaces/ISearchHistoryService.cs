using MomEase.core.DTOS.ArticlesDTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MomEase.core.Interfaces
{
    public interface ISearchHistoryService
    {
        Task<SearchHistoryDto> AddSearchAsync(int userId, string searchTerm);
        Task<IEnumerable<SearchHistoryDto>> GetHistoryAsync(int userId);
        Task<bool> ClearAllHistoryAsync(int userId);
        Task<bool> DeleteSearchTermAsync(int userId, string searchTerm);
    }
}
