using Microsoft.AspNetCore.Http;
using MomEase.core.DTOS.ArticleCategories;
using MomEase.core.Entities;
using MomEase.core.Interfaces;
using MomEase.infra.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MomEase.infra.Services
{
    public class ArticleCategoryService : IArticleCategoryService
    {
        private readonly IArticleCategoryRepository _repository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public ArticleCategoryService(IArticleCategoryRepository repository,
        IHttpContextAccessor httpContextAccessor)
        {
            _repository = repository;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<ArticleCategoryDto> CreateAsync(CreateArticleCategoryDto dto)
        {
            var nameExists = await _repository.NameExistsAsync(dto.Name);
            if (nameExists)
                throw new Exception("Category name already exists");

            var category = new ArticleCategories
            {
                Name = dto.Name,
                Description = dto.Description,
                ImageUrl = dto.ImageUrl  // ← أضف ده
            };

            var created = await _repository.CreateAsync(category);

            return new ArticleCategoryDto
            {
                CategoryId = created.CategoryId,
                Name = created.Name,
                Description = created.Description,
                ImageUrl = created.ImageUrl,  // ← أضف ده
                ArticlesCount = 0
            };
        }
        public async Task<ArticleCategoryDto> GetByIdAsync(int categoryId)
        {
            var category = await _repository.GetByIdAsync(categoryId);
            if (category == null)
                throw new Exception("Category not found");
            var lang = GetLang();
            return new ArticleCategoryDto
            {
                CategoryId = category.CategoryId,
                Name = LanguageHelper.GetLocalized(category.NameAr, category.Name, lang), // ← غيري
                Description = LanguageHelper.GetLocalized(category.DescriptionAr, category.Description, lang), // ← غيري
                ImageUrl = category.ImageUrl,
                ArticlesCount = category.Articles?.Count ?? 0
            };
        }
        public async Task<IEnumerable<ArticleCategoryDto>> GetAllAsync()
        {
            var categories = await _repository.GetAllAsync();
            var lang = GetLang();
            return categories.Select(c => new ArticleCategoryDto
            {
                CategoryId = c.CategoryId,
                Name = LanguageHelper.GetLocalized(c.NameAr, c.Name, lang), // ← غيري
                Description = LanguageHelper.GetLocalized(c.DescriptionAr, c.Description, lang), // ← غيري
                ImageUrl = c.ImageUrl,
                ArticlesCount = c.Articles?.Count ?? 0
            });
        }
        public async Task<ArticleCategoryDto> UpdateAsync(int categoryId, UpdateArticleCategoryDto dto)
        {
            var category = await _repository.GetByIdAsync(categoryId);
            if (category == null)
                throw new Exception("Category not found");

            if (!string.IsNullOrWhiteSpace(dto.Name) && dto.Name != "string" && dto.Name != category.Name)
            {
                var nameExists = await _repository.NameExistsAsync(dto.Name, categoryId);
                if (nameExists)
                    throw new Exception("Category name already exists");

                category.Name = dto.Name;
            }

            if (!string.IsNullOrWhiteSpace(dto.Description) && dto.Description != "string")
                category.Description = dto.Description;

            if (!string.IsNullOrWhiteSpace(dto.ImageUrl) && dto.ImageUrl != "string")
                category.ImageUrl = dto.ImageUrl;

            var updated = await _repository.UpdateAsync(category);

            return new ArticleCategoryDto
            {
                CategoryId = updated.CategoryId,
                Name = updated.Name,
                Description = updated.Description,
                ImageUrl = updated.ImageUrl,  // ← أضف ده
                ArticlesCount = updated.Articles?.Count ?? 0
            };
        }
        public async Task<bool> DeleteAsync(int categoryId)
        {
            var exists = await _repository.ExistsAsync(categoryId);
            if (!exists)
                throw new Exception("Category not found");

            // تحقق من عدم وجود مقالات
            var articlesCount = await _repository.GetArticlesCountAsync(categoryId);
            if (articlesCount > 0)
                throw new Exception($"Cannot delete category with {articlesCount} articles");

            return await _repository.DeleteAsync(categoryId);
        }
        private string GetLang()
        {
            return LanguageHelper.GetLang(_httpContextAccessor);
        }
    }
}
