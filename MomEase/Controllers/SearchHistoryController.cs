using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MomEase.core.Interfaces;
using System.Security.Claims;

namespace MomEase.api.Controllers
{
    [Route("api/search/history")]
    [ApiController]
    [Authorize] // كل الـ endpoints تحتاج تسجيل دخول
    public class SearchHistoryController : ControllerBase
    {
        private readonly ISearchHistoryService _searchHistoryService;

        public SearchHistoryController(ISearchHistoryService searchHistoryService)
        {
            _searchHistoryService = searchHistoryService;
        }

        /// <summary>
        /// GET /api/search/history
        /// عرض تاريخ البحث للمستخدم (آخر 10 عمليات بحث)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetSearchHistory()
        {
            try
            {
                var userId = GetCurrentUserId();

                if (!userId.HasValue)
                    return Unauthorized(new { success = false, message = "User not authenticated" });

                var history = await _searchHistoryService.GetHistoryAsync(userId.Value);

                return Ok(new
                {
                    success = true,
                    data = history
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// DELETE /api/search/history
        /// حذف كل تاريخ البحث للمستخدم
        /// </summary>
        [HttpDelete]
        public async Task<IActionResult> ClearAllHistory()
        {
            try
            {
                var userId = GetCurrentUserId();

                if (!userId.HasValue)
                    return Unauthorized(new { success = false, message = "User not authenticated" });

                var deleted = await _searchHistoryService.ClearAllHistoryAsync(userId.Value);

                if (!deleted)
                    return NotFound(new { success = false, message = "No search history found" });

                return Ok(new
                {
                    success = true,
                    message = "All search history deleted successfully"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// DELETE /api/search/history/{term}
        /// حذف كلمة بحث محددة من التاريخ
        /// </summary>
        [HttpDelete("{term}")]
        public async Task<IActionResult> DeleteSearchTerm(string term)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(term))
                    return BadRequest(new { success = false, message = "Search term cannot be empty" });

                var userId = GetCurrentUserId();

                if (!userId.HasValue)
                    return Unauthorized(new { success = false, message = "User not authenticated" });

                var deleted = await _searchHistoryService.DeleteSearchTermAsync(userId.Value, term);

                if (!deleted)
                    return NotFound(new { success = false, message = "Search term not found in history" });

                return Ok(new
                {
                    success = true,
                    message = $"Search term '{term}' deleted successfully"
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
