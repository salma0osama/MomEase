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
    public class ArticleRepository : IArticleRepository
    {
        private readonly MomEaseDbContext _context;

        public ArticleRepository(MomEaseDbContext context)
        {
            _context = context;
        }

        public async Task<Articles> CreateAsync(Articles article)
        {
            article.PublishedDate = DateTime.Now;
            await _context.Articles.AddAsync(article);
            await _context.SaveChangesAsync();
            return article;
        }

        public async Task<Articles> GetByIdAsync(int articleId)
        {
            return await _context.Articles
                .Include(a => a.Category)
                .FirstOrDefaultAsync(a => a.ArticleId == articleId);
        }

        public async Task<IEnumerable<Articles>> GetAllAsync()
        {
            return await _context.Articles
                .Include(a => a.Category)
                .OrderByDescending(a => a.PublishedDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Articles>> GetByCategoryIdAsync(int categoryId)
        {
            return await _context.Articles
                .Include(a => a.Category)
                .Where(a => a.CategoryId == categoryId)
                .OrderByDescending(a => a.PublishedDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Articles>> SearchAsync(string searchTerm)
        {
            return await _context.Articles
                .Include(a => a.Category)
                .Where(a => a.Title.Contains(searchTerm))  // ← بس في الـ Title
                .OrderByDescending(a => a.PublishedDate)
                .ToListAsync();
        }

        public async Task<Articles> UpdateAsync(Articles article)
        {
            _context.Articles.Update(article);
            await _context.SaveChangesAsync();
            return article;
        }

        public async Task<bool> DeleteAsync(int articleId)
        {
            var article = await _context.Articles.FindAsync(articleId);
            if (article == null) return false;

            _context.Articles.Remove(article);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(int articleId)
        {
            return await _context.Articles.AnyAsync(a => a.ArticleId == articleId);
        }
    }
}
