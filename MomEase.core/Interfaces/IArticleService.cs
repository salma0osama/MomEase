using MomEase.core.DTOS.ArticlesDTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MomEase.core.Interfaces
{
    public interface IArticleService
    {
        Task<ArticleDto> CreateAsync(CreateArticleDto dto);
        Task<ArticleDto> GetByIdAsync(int articleId, int? userId = null);
        Task<IEnumerable<ArticleListDto>> GetAllAsync(int? userId = null);
        Task<IEnumerable<ArticleListDto>> GetByCategoryIdAsync(int categoryId, int? userId = null);
        Task<SearchResultDto> SearchAsync(string searchTerm, int? userId = null);
        Task<ArticleDto> UpdateAsync(int articleId, UpdateArticleDto dto);
        Task<bool> DeleteAsync(int articleId);
    }
}
