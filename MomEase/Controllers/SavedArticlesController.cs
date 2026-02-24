using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MomEase.core.DTOS.ArticlesDTOs;
using MomEase.core.Interfaces;
using System.Security.Claims;

namespace MomEase.api.Controllers
{
    [Route("api/saved-articles")]
    [ApiController]
    [Authorize] // كل الـ endpoints تحتاج تسجيل دخول
    public class SavedArticlesController : ControllerBase
    {
        private readonly ISavedArticleService _savedArticleService;

        public SavedArticlesController(ISavedArticleService savedArticleService)
        {
            _savedArticleService = savedArticleService;
        }

        /// <summary>
        /// GET /api/saved-articles
        /// عرض المقالات المحفوظة للمستخدم
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetSavedArticles()
        {
            try
            {
                var userId = GetCurrentUserId();

                if (!userId.HasValue)
                    return Unauthorized(new { success = false, message = "User not authenticated" });

                var savedArticles = await _savedArticleService.GetSavedArticlesAsync(userId.Value);

                return Ok(new
                {
                    success = true,
                    data = savedArticles
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// POST /api/saved-articles
        /// حفظ مقالة (Bookmark)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> SaveArticle([FromBody] SaveArticleRequestDto requestDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new { success = false, errors = ModelState });

                var userId = GetCurrentUserId();

                if (!userId.HasValue)
                    return Unauthorized(new { success = false, message = "User not authenticated" });

                var savedArticle = await _savedArticleService.SaveArticleAsync(userId.Value, requestDto.ArticleId);

                return Ok(new
                {
                    success = true,
                    message = "Article saved successfully",
                    data = savedArticle
                });
            }
            catch (Exception ex)
            {
                // يمكن يكون الخطأ "Article is already saved" أو "Article not found"
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// DELETE /api/saved-articles/{articleId}
        /// إلغاء حفظ مقالة (Unbookmark)
        /// </summary>
        [HttpDelete("{articleId}")]
        public async Task<IActionResult> UnsaveArticle(int articleId)
        {
            try
            {
                if (articleId <= 0)
                    return BadRequest(new { success = false, message = "Invalid article ID" });

                var userId = GetCurrentUserId();

                if (!userId.HasValue)
                    return Unauthorized(new { success = false, message = "User not authenticated" });

                var deleted = await _savedArticleService.UnsaveArticleAsync(userId.Value, articleId);

                if (!deleted)
                    return NotFound(new { success = false, message = "Saved article not found" });

                return Ok(new
                {
                    success = true,
                    message = "Article unsaved successfully"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        #region Helper Methods

        /// <summary>
        /// جلب ID المستخدم الحالي من JWT Token
        /// </summary>
        private int? GetCurrentUserId()
        {
            var userIdClaim = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (int.TryParse(userIdClaim, out int userId))
                return userId;

            return null;
        }

        #endregion
    }
}
