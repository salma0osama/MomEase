using MomEase.core.DTOS.ArticleCategories;
using MomEase.core.Entities;
using MomEase.core.Interfaces;
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

        public ArticleCategoryService(IArticleCategoryRepository repository)
        {
            _repository = repository;
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

            return new ArticleCategoryDto
            {
                CategoryId = category.CategoryId,
                Name = category.Name,
                Description = category.Description,
                ImageUrl = category.ImageUrl,  // ← أضف ده
                ArticlesCount = category.Articles?.Count ?? 0
            };
        }
        public async Task<IEnumerable<ArticleCategoryDto>> GetAllAsync()
        {
            var categories = await _repository.GetAllAsync();

            return categories.Select(c => new ArticleCategoryDto
            {
                CategoryId = c.CategoryId,
                Name = c.Name,
                Description = c.Description,
                ImageUrl = c.ImageUrl,  // ← أضف ده
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
    }
}
