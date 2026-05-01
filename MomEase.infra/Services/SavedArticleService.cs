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
    public class SavedArticleService : ISavedArticleService
    {
        private readonly ISavedArticleRepository _savedArticleRepo;
        private readonly IArticleRepository _articleRepo;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public SavedArticleService(
            ISavedArticleRepository savedArticleRepo,
            IArticleRepository articleRepo,
            IHttpContextAccessor httpContextAccessor)
        {
            _savedArticleRepo = savedArticleRepo;
            _articleRepo = articleRepo;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<SavedArticleDto> SaveArticleAsync(int userId, int articleId)
        {
            // تحقق من وجود المقالة
            var articleExists = await _articleRepo.ExistsAsync(articleId);
            if (!articleExists)
                throw new Exception("Article not found");

            // تحقق إذا كانت محفوظة بالفعل
            var alreadySaved = await _savedArticleRepo.ExistsAsync(userId, articleId);
            if (alreadySaved)
                throw new Exception("Article is already saved");

            var savedArticle = new SavedArticles
            {
                UserId = userId,
                ArticleId = articleId,
                SavedAt = DateTime.Now
            };

            var created = await _savedArticleRepo.CreateAsync(savedArticle);
            var article = await _articleRepo.GetByIdAsync(articleId);

            return new SavedArticleDto
            {
                SavedArticleId = created.SavedArticleId,
                ArticleId = article.ArticleId,
                Title = article.Title,
                ImageUrl = article.ImageUrl,
                CategoryName = article.Category?.Name,
                ReadingTimeMinutes = CalculateReadingTime(article.Content),
                SavedAt = created.SavedAt
            };
        }

        public async Task<IEnumerable<SavedArticleDto>> GetSavedArticlesAsync(int userId)
        {
            var savedArticles = await _savedArticleRepo.GetByUserIdAsync(userId);
            var lang = GetLang(); // ✅ أضف

            return savedArticles.Select(s => new SavedArticleDto
            {
                SavedArticleId = s.SavedArticleId,
                ArticleId = s.Article.ArticleId,
                Title = LanguageHelper.GetLocalized(s.Article.TitleAr, s.Article.Title, lang),             // ✅
                Content = LanguageHelper.GetLocalized(s.Article.ContentAr, s.Article.Content, lang), // ✅ 
                ImageUrl = s.Article.ImageUrl,
                CategoryName = LanguageHelper.GetLocalized(s.Article.Category?.NameAr, s.Article.Category?.Name, lang), // ✅
                ReadingTimeMinutes = CalculateReadingTime(
                    LanguageHelper.GetLocalized(s.Article.ContentAr, s.Article.Content, lang)),             // ✅
                SavedAt = s.SavedAt
            });
        }

        public async Task<bool> UnsaveArticleAsync(int userId, int articleId)
        {
            var exists = await _savedArticleRepo.ExistsAsync(userId, articleId);
            if (!exists)
                throw new Exception("Article is not saved");

            return await _savedArticleRepo.DeleteAsync(userId, articleId);
        }

        private int CalculateReadingTime(string content)
        {
            if (string.IsNullOrEmpty(content))
                return 0;

            int wordCount = content.Split(new[] { ' ', '\n', '\r' },
                StringSplitOptions.RemoveEmptyEntries).Length;
            return (int)Math.Ceiling(wordCount / 200.0);
        }
        private string GetLang()
        {
            return LanguageHelper.GetLang(_httpContextAccessor);
        }
    }
}
