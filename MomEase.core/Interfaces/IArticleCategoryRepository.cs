using MomEase.core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MomEase.core.Interfaces
{
    public interface IArticleCategoryRepository
    {
        Task<ArticleCategories> CreateAsync(ArticleCategories category);
        Task<ArticleCategories> GetByIdAsync(int categoryId);
        Task<IEnumerable<ArticleCategories>> GetAllAsync();
        Task<ArticleCategories> UpdateAsync(ArticleCategories category);
        Task<bool> DeleteAsync(int categoryId);
        Task<bool> ExistsAsync(int categoryId);
        Task<IEnumerable<ArticleCategories>> SearchAsync(string searchTerm);

        Task<bool> NameExistsAsync(string name, int? excludeId = null);
        Task<int> GetArticlesCountAsync(int categoryId);
    }
}
