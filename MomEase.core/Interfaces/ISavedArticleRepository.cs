using MomEase.core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MomEase.core.Interfaces
{
    public interface ISavedArticleRepository
    {
        Task<SavedArticles> CreateAsync(SavedArticles savedArticle);
        Task<IEnumerable<SavedArticles>> GetByUserIdAsync(int userId);
        Task<bool> DeleteAsync(int userId, int articleId);
        Task<bool> ExistsAsync(int userId, int articleId);
    }
}
