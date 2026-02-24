using MomEase.core.DTOS.ArticlesDTOs;
using MomEase.core.Entities;
using MomEase.core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MomEase.infra.Services
{
    public class ArticleService : IArticleService
    {
        private readonly IArticleRepository _articleRepo;
        private readonly IArticleCategoryRepository _categoryRepo;
        private readonly ISavedArticleRepository _savedArticleRepo;

        public ArticleService(
            IArticleRepository articleRepo,
            IArticleCategoryRepository categoryRepo,
            ISavedArticleRepository savedArticleRepo)
        {
            _articleRepo = articleRepo;
            _categoryRepo = categoryRepo;
            _savedArticleRepo = savedArticleRepo;
        }

        public async Task<ArticleDto> CreateAsync(CreateArticleDto dto)
        {
            var categoryExists = await _categoryRepo.ExistsAsync(dto.CategoryId);
            if (!categoryExists)
                throw new Exception("Category not found");

            var article = new Articles
            {
                CategoryId = dto.CategoryId,
                Title = dto.Title,
                Content = dto.Content,
                ImageUrl = dto.ImageUrl,
                SourceUrl = dto.SourceUrl,
                SourceName = dto.SourceName
            };

            var created = await _articleRepo.CreateAsync(article);
            var category = await _categoryRepo.GetByIdAsync(dto.CategoryId);

            return new ArticleDto
            {
                ArticleId = created.ArticleId,
                Title = created.Title,
                Content = created.Content,
                ImageUrl = created.ImageUrl,
                CategoryName = category.Name,
                CategoryId = created.CategoryId,
                ReadingTimeMinutes = CalculateReadingTime(created.Content),
                PublishedDate = created.PublishedDate,
                SourceUrl = created.SourceUrl,
                SourceName = created.SourceName,
                IsSaved = false
            };
        }

        public async Task<ArticleDto> GetByIdAsync(int articleId, int? userId = null)
        {
            var article = await _articleRepo.GetByIdAsync(articleId);
            if (article == null)
                throw new Exception("Article not found");

            bool isSaved = false;
            if (userId.HasValue)
            {
                isSaved = await _savedArticleRepo.ExistsAsync(userId.Value, articleId);
            }

            return new ArticleDto
            {
                ArticleId = article.ArticleId,
                Title = article.Title,
                Content = article.Content,
                ImageUrl = article.ImageUrl,
                CategoryName = article.Category?.Name,
                CategoryId = article.CategoryId,
                ReadingTimeMinutes = CalculateReadingTime(article.Content),
                PublishedDate = article.PublishedDate,
                SourceUrl = article.SourceUrl,
                SourceName = article.SourceName,
                IsSaved = isSaved
            };
        }

        public async Task<IEnumerable<ArticleListDto>> GetAllAsync(int? userId = null)
        {
            var articles = await _articleRepo.GetAllAsync();
            return await MapToListDto(articles, userId);
        }

        public async Task<IEnumerable<ArticleListDto>> GetByCategoryIdAsync(int categoryId, int? userId = null)
        {
            var categoryExists = await _categoryRepo.ExistsAsync(categoryId);
            if (!categoryExists)
                throw new Exception("Category not found");

            var articles = await _articleRepo.GetByCategoryIdAsync(categoryId);
            return await MapToListDto(articles, userId);
        }

        public async Task<SearchResultDto> SearchAsync(string searchTerm, int? userId = null)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return new SearchResultDto
                {
                    Articles = new List<ArticleListDto>(),
                    Categories = new List<CategoryResultDto>()
                };

            // 1️⃣ البحث في المقالات
            var articles = await _articleRepo.SearchAsync(searchTerm);
            var articleDtos = await MapToListDto(articles, userId);

            // 2️⃣ البحث في الفئات
            var categories = await _categoryRepo.SearchAsync(searchTerm);
            var categoryDtos = categories.Select(c => new CategoryResultDto
            {
                CategoryId = c.CategoryId,
                Name = c.Name,
                Description = c.Description,
                ImageUrl = c.ImageUrl,
                ArticlesCount = c.Articles?.Count ?? 0
            });

            // 3️⃣ إرجاع النتيجة الشاملة
            return new SearchResultDto
            {
                Articles = articleDtos,
                Categories = categoryDtos
            };
        }

        public async Task<ArticleDto> UpdateAsync(int articleId, UpdateArticleDto dto)
        {
            var article = await _articleRepo.GetByIdAsync(articleId);
            if (article == null)
                throw new Exception("Article not found");

            if (dto.CategoryId.HasValue)
            {
                var categoryExists = await _categoryRepo.ExistsAsync(dto.CategoryId.Value);
                if (!categoryExists)
                    throw new Exception("Category not found");
                article.CategoryId = dto.CategoryId.Value;
            }

            if (!string.IsNullOrWhiteSpace(dto.Title))
                article.Title = dto.Title;

            if (!string.IsNullOrWhiteSpace(dto.Content))
                article.Content = dto.Content;

            if (!string.IsNullOrWhiteSpace(dto.ImageUrl))
                article.ImageUrl = dto.ImageUrl;

            if (!string.IsNullOrWhiteSpace(dto.SourceUrl))
                article.SourceUrl = dto.SourceUrl;

            if (!string.IsNullOrWhiteSpace(dto.SourceName))
                article.SourceName = dto.SourceName;

            var updated = await _articleRepo.UpdateAsync(article);

            return await GetByIdAsync(updated.ArticleId);
        }

        public async Task<bool> DeleteAsync(int articleId)
        {
            var exists = await _articleRepo.ExistsAsync(articleId);
            if (!exists)
                throw new Exception("Article not found");

            return await _articleRepo.DeleteAsync(articleId);
        }

        // Helper Methods
        private async Task<IEnumerable<ArticleListDto>> MapToListDto(IEnumerable<Articles> articles, int? userId)
        {
            var result = new List<ArticleListDto>();

            foreach (var article in articles)
            {
                bool isSaved = false;
                if (userId.HasValue)
                {
                    isSaved = await _savedArticleRepo.ExistsAsync(userId.Value, article.ArticleId);
                }

                result.Add(new ArticleListDto
                {
                    ArticleId = article.ArticleId,
                    Title = article.Title,
                    ImageUrl = article.ImageUrl,
                    ShortDescription = GetShortDescription(article.Content),
                    CategoryName = article.Category?.Name,
                    CategoryId = article.CategoryId,
                    ReadingTimeMinutes = CalculateReadingTime(article.Content),
                    IsSaved = isSaved
                });
            }

            return result;
        }

        private string GetShortDescription(string content)
        {
            if (string.IsNullOrEmpty(content))
                return string.Empty;

            return content.Length <= 150
                ? content
                : content.Substring(0, 150) + "...";
        }

        private int CalculateReadingTime(string content)
        {
            if (string.IsNullOrEmpty(content))
                return 0;

            int wordCount = content.Split(new[] { ' ', '\n', '\r' },
                StringSplitOptions.RemoveEmptyEntries).Length;
            return (int)Math.Ceiling(wordCount / 200.0); // 200 words per minute
        }
    }
}
