using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MomEase.core.DTOS.ArticlesDTOs;
using MomEase.core.Interfaces;
using System.Security.Claims;

namespace MomEase.api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ArticlesController : ControllerBase
    {
        private readonly IArticleService _articleService;
        private readonly ISearchHistoryService _searchHistoryService;

        public ArticlesController(
            IArticleService articleService,
            ISearchHistoryService searchHistoryService)
        {
            _articleService = articleService;
            _searchHistoryService = searchHistoryService;
        }

        #region Public Endpoints

        /// <summary>
        /// GET /api/articles
        /// جلب كل المقالات
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllArticles()
        {
            try
            {
                var userId = GetCurrentUserId();
                var articles = await _articleService.GetAllAsync(userId);

                return Ok(new { success = true, data = articles });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// GET /api/articles/category/{categoryId}
        /// جلب مقالات فئة معينة
        /// </summary>
        [HttpGet("category/{categoryId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetArticlesByCategory(int categoryId)
        {
            try
            {
                if (categoryId <= 0)
                    return BadRequest(new { success = false, message = "Invalid category ID" });

                var userId = GetCurrentUserId();
                var articles = await _articleService.GetByCategoryIdAsync(categoryId, userId);

                return Ok(new { success = true, data = articles });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// GET /api/articles/{id}
        /// جلب تفاصيل مقالة واحدة
        /// </summary>
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetArticleById(int id)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(new { success = false, message = "Invalid article ID" });

                var userId = GetCurrentUserId();
                var article = await _articleService.GetByIdAsync(id, userId);

                return Ok(new
                {
                    success = true,
                    data = article
                });
            }
            catch (Exception ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// GET /api/articles/search?query={text}
        /// البحث في المقالات + حفظ في تاريخ البحث تلقائياً
        /// </summary>
        [HttpGet("search")]
        [AllowAnonymous] // يمكن البحث بدون تسجيل دخول، لكن التاريخ يُحفظ فقط للمسجلين
        public async Task<IActionResult> SearchArticles([FromQuery] string query)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(query))
                    return BadRequest(new { success = false, message = "Search query cannot be empty" });

                var userId = GetCurrentUserId();

                // البحث في المقالات والفئات
                var searchResult = await _articleService.SearchAsync(query, userId);

                // حفظ في تاريخ البحث (فقط للمستخدمين المسجلين)
                if (userId.HasValue)
                {
                    try
                    {
                        await _searchHistoryService.AddSearchAsync(userId.Value, query);
                    }
                    catch
                    {
                        // نتجاهل أي خطأ في حفظ التاريخ
                    }
                }

                return Ok(new
                {
                    success = true,
                    searchTerm = query,
                    data = new
                    {
                        articles = searchResult.Articles,
                        categories = searchResult.Categories,
                        articlesCount = searchResult.Articles?.Count() ?? 0,
                        categoriesCount = searchResult.Categories?.Count() ?? 0
                    }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }        
        #region Admin Endpoints

        /// <summary>
        /// POST /api/articles
        /// إضافة مقالة جديدة (Admin only)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> CreateArticle([FromBody] CreateArticleDto createDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new { success = false, errors = ModelState });

                var article = await _articleService.CreateAsync(createDto);

                return CreatedAtAction(
                    nameof(GetArticleById),
                    new { id = article.ArticleId },
                    new { success = true, data = article }
                );
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// PUT /api/articles/{id}
        /// تحديث مقالة (Admin only)
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> UpdateArticle(int id, [FromBody] UpdateArticleDto updateDto)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(new { success = false, message = "Invalid article ID" });

                if (!ModelState.IsValid)
                    return BadRequest(new { success = false, errors = ModelState });

                var article = await _articleService.UpdateAsync(id, updateDto);

                return Ok(new { success = true, data = article });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// DELETE /api/articles/{id}
        /// حذف مقالة (Admin only)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> DeleteArticle(int id)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(new { success = false, message = "Invalid article ID" });

                var deleted = await _articleService.DeleteAsync(id);

                if (!deleted)
                    return NotFound(new { success = false, message = "Article not found" });

                return Ok(new { success = true, message = "Article deleted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        #endregion

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
#endregion