using Microsoft.AspNetCore.Http;
using MomEase.core.DTOS.ArticlesDTOs;
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
    public class ArticleService : IArticleService
    {
        private readonly IArticleRepository _articleRepo;
        private readonly IArticleCategoryRepository _categoryRepo;
        private readonly ISavedArticleRepository _savedArticleRepo;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ArticleService(
            IArticleRepository articleRepo,
            IArticleCategoryRepository categoryRepo,
            ISavedArticleRepository savedArticleRepo,
            IHttpContextAccessor httpContextAccessor)  // ⭐ إضافة
        {
            _articleRepo = articleRepo;
            _categoryRepo = categoryRepo;
            _savedArticleRepo = savedArticleRepo;
            _httpContextAccessor = httpContextAccessor;  // ⭐ إضافة
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
                TitleAr = dto.TitleAr,        // ⭐ جديد
                ContentAr = dto.ContentAr,    // ⭐ جديد
                ImageUrl = dto.ImageUrl,
                SourceUrl = dto.SourceUrl,
                SourceName = dto.SourceName
            };

            var created = await _articleRepo.CreateAsync(article);

            // Return with localization
            return await GetByIdAsync(created.ArticleId);
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

            // ⭐ Get language from request
            var lang = GetLang();

            // ⭐ Use localized content
            var localizedTitle = LanguageHelper.GetLocalized(
                article.TitleAr,
                article.Title,
                lang);

            var localizedContent = LanguageHelper.GetLocalized(
                article.ContentAr,
                article.Content,
                lang);

            var localizedCategoryName = LanguageHelper.GetLocalized(
                article.Category?.NameAr,
                article.Category?.Name,
                lang);

            return new ArticleDto
            {
                ArticleId = article.ArticleId,
                Title = localizedTitle,         // ⭐ محلّي
                Content = localizedContent,     // ⭐ محلّي
                ImageUrl = article.ImageUrl,
                CategoryName = localizedCategoryName,  // ⭐ محلّي
                CategoryId = article.CategoryId,
                ReadingTimeMinutes = CalculateReadingTime(localizedContent),
                PublishedDate = article.PublishedDate ?? DateTime.Now.AddHours(1),
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

            // ⭐ Get language
            var lang = GetLang();

            // 1️⃣ البحث في المقالات
            var articles = await _articleRepo.SearchAsync(searchTerm);
            var articleDtos = await MapToListDto(articles, userId);

            // 2️⃣ البحث في الفئات
            var categories = await _categoryRepo.SearchAsync(searchTerm);
            var categoryDtos = categories.Select(c => new CategoryResultDto
            {
                CategoryId = c.CategoryId,
                Name = LanguageHelper.GetLocalized(c.NameAr, c.Name, lang),  // ⭐ محلّي
                Description = LanguageHelper.GetLocalized(c.DescriptionAr, c.Description, lang),  // ⭐ محلّي
                ImageUrl = c.ImageUrl,
                ArticlesCount = c.Articles?.Count ?? 0
            });

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

            // Update English
            if (!string.IsNullOrWhiteSpace(dto.Title))
                article.Title = dto.Title;

            if (!string.IsNullOrWhiteSpace(dto.Content))
                article.Content = dto.Content;

            // ⭐ Update Arabic
            if (dto.TitleAr != null)
                article.TitleAr = dto.TitleAr;

            if (dto.ContentAr != null)
                article.ContentAr = dto.ContentAr;

            // Update common fields
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

        // ⭐ Helper Methods
        private async Task<IEnumerable<ArticleListDto>> MapToListDto(IEnumerable<Articles> articles, int? userId)
        {
            var result = new List<ArticleListDto>();
            var lang = GetLang();  // ⭐ Get language once

            foreach (var article in articles)
            {
                bool isSaved = false;
                if (userId.HasValue)
                {
                    isSaved = await _savedArticleRepo.ExistsAsync(userId.Value, article.ArticleId);
                }

                // ⭐ Localize content
                var localizedTitle = LanguageHelper.GetLocalized(
                    article.TitleAr,
                    article.Title,
                    lang);

                var localizedContent = LanguageHelper.GetLocalized(
                    article.ContentAr,
                    article.Content,
                    lang);

                var localizedCategoryName = LanguageHelper.GetLocalized(
                    article.Category?.NameAr,
                    article.Category?.Name,
                    lang);

                result.Add(new ArticleListDto
                {
                    ArticleId = article.ArticleId,
                    Title = localizedTitle,  // ⭐ محلّي
                    ImageUrl = article.ImageUrl,
                    ShortDescription = GetShortDescription(localizedContent),  // ⭐ من المحتوى المحلّي
                    CategoryName = localizedCategoryName,  // ⭐ محلّي
                    CategoryId = article.CategoryId,
                    ReadingTimeMinutes = CalculateReadingTime(localizedContent),  // ⭐ من المحتوى المحلّي
                    IsSaved = isSaved
                });
            }

            return result;
        }

        /// <summary>
        /// ⭐ Get current language from HTTP request
        /// </summary>
        private string GetLang()
        {
            return LanguageHelper.GetLang(_httpContextAccessor);
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
            return (int)Math.Ceiling(wordCount / 200.0);
        }
    }
}
