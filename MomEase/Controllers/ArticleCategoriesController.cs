using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MomEase.core.DTOS.ArticleCategories;
using MomEase.core.Interfaces;

namespace MomEase.api.Controllers
{
    [Route("api/articles/categories")]
    [ApiController]
    public class ArticleCategoriesController : ControllerBase
    {
        private readonly IArticleCategoryService _service;

        public ArticleCategoriesController(IArticleCategoryService service)
        {
            _service = service;
        }

        /// <summary>
        /// الحصول على جميع الفئات
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var categories = await _service.GetAllAsync();

                return Ok(new
                {
                    success = true,
                    count = categories.Count(),
                    data = categories
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// الحصول على فئة معينة
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var category = await _service.GetByIdAsync(id);

                return Ok(new
                {
                    success = true,
                    data = category
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// إضافة فئة جديدة (Admin فقط)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Create([FromBody] CreateArticleCategoryDto dto)
        {
            try
            {
                var category = await _service.CreateAsync(dto);

                return Ok(new
                {
                    success = true,
                    message = "Category created successfully",
                    data = category
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// تحديث فئة (Admin فقط)
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateArticleCategoryDto dto)
        {
            try
            {
                var category = await _service.UpdateAsync(id, dto);

                return Ok(new
                {
                    success = true,
                    message = "Category updated successfully",
                    data = category
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// حذف فئة (Admin فقط)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _service.DeleteAsync(id);

                return Ok(new
                {
                    success = true,
                    message = "Category deleted successfully"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }
}
