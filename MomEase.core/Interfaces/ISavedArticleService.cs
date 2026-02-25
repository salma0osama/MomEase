using MomEase.core.DTOS.ArticlesDTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MomEase.core.Interfaces
{
    public interface ISavedArticleService
    {
        Task<SavedArticleDto> SaveArticleAsync(int userId, int articleId);
        Task<IEnumerable<SavedArticleDto>> GetSavedArticlesAsync(int userId);
        Task<bool> UnsaveArticleAsync(int userId, int articleId);
    }
}
