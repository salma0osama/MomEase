using MomEase.core.DTOS.ArticleCategories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MomEase.core.Interfaces
{
    public interface IArticleCategoryService
    {
        Task<ArticleCategoryDto> CreateAsync(CreateArticleCategoryDto dto);
        Task<ArticleCategoryDto> GetByIdAsync(int categoryId);
        Task<IEnumerable<ArticleCategoryDto>> GetAllAsync();
        Task<ArticleCategoryDto> UpdateAsync(int categoryId, UpdateArticleCategoryDto dto);
        Task<bool> DeleteAsync(int categoryId);
    }
}
