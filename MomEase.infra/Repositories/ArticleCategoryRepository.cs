using Google;
using Microsoft.EntityFrameworkCore;
using MomEase.core.Entities;
using MomEase.core.Interfaces;
using MomEase.infra.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MomEase.infra.Repositories
{
    public class ArticleCategoryRepository : IArticleCategoryRepository
    {
        private readonly MomEaseDbContext _context;

        public ArticleCategoryRepository(MomEaseDbContext context)
        {
            _context = context;
        }

        public async Task<ArticleCategories> CreateAsync(ArticleCategories category)
        {
            await _context.ArticleCategories.AddAsync(category);
            await _context.SaveChangesAsync();
            return category;
        }

        public async Task<ArticleCategories> GetByIdAsync(int categoryId)
        {
            return await _context.ArticleCategories
                .Include(c => c.Articles)
                .FirstOrDefaultAsync(c => c.CategoryId == categoryId);
        }

        public async Task<IEnumerable<ArticleCategories>> GetAllAsync()
        {
            return await _context.ArticleCategories
                .Include(c => c.Articles)
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<ArticleCategories> UpdateAsync(ArticleCategories category)
        {
            _context.ArticleCategories.Update(category);
            await _context.SaveChangesAsync();
            return category;
        }

        public async Task<bool> DeleteAsync(int categoryId)
        {
            var category = await _context.ArticleCategories.FindAsync(categoryId);
            if (category == null) return false;

            _context.ArticleCategories.Remove(category);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(int categoryId)
        {
            return await _context.ArticleCategories
                .AnyAsync(c => c.CategoryId == categoryId);
        }
        public async Task<IEnumerable<ArticleCategories>> SearchAsync(string searchTerm)
        {
            return await _context.ArticleCategories
        .Include(c => c.Articles)
        .Where(c => c.Name.Contains(searchTerm)
                 || c.NameAr.Contains(searchTerm)         // ⭐ أضيفي
                 || c.Description.Contains(searchTerm)    // ⭐ أضيفي
                 || c.DescriptionAr.Contains(searchTerm)) // ⭐ أضيفي
        .ToListAsync();
        }
        public async Task<bool> NameExistsAsync(string name, int? excludeId = null)
        {
            var query = _context.ArticleCategories
                .Where(c => c.Name.ToLower() == name.ToLower());

            if (excludeId.HasValue)
            {
                query = query.Where(c => c.CategoryId != excludeId.Value);
            }

            return await query.AnyAsync();
        }

        public async Task<int> GetArticlesCountAsync(int categoryId)
        {
            return await _context.Articles
                .CountAsync(a => a.CategoryId == categoryId);
        }
    }
    }
