using MomEase.core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MomEase.core.Interfaces
{
    public interface IArticleRepository
    {
        Task<Articles> CreateAsync(Articles article);
        Task<Articles> GetByIdAsync(int articleId);
        Task<IEnumerable<Articles>> GetAllAsync();
        Task<IEnumerable<Articles>> GetByCategoryIdAsync(int categoryId);
        Task<IEnumerable<Articles>> SearchAsync(string searchTerm);
        Task<Articles> UpdateAsync(Articles article);
        Task<bool> DeleteAsync(int articleId);
        Task<bool> ExistsAsync(int articleId);
    }
}
