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
    public class SavedArticleRepository : ISavedArticleRepository
    {
        private readonly MomEaseDbContext _context;

        public SavedArticleRepository(MomEaseDbContext context)
        {
            _context = context;
        }

        public async Task<SavedArticles> CreateAsync(SavedArticles savedArticle)
        {
            await _context.SavedArticles.AddAsync(savedArticle);
            await _context.SaveChangesAsync();
            return savedArticle;
        }

        public async Task<IEnumerable<SavedArticles>> GetByUserIdAsync(int userId)
        {
            return await _context.SavedArticles
                .Include(s => s.Article)
                    .ThenInclude(a => a.Category)
                .Where(s => s.UserId == userId)
                .OrderByDescending(s => s.SavedAt)
                .ToListAsync();
        }

        public async Task<bool> DeleteAsync(int userId, int articleId)
        {
            var savedArticle = await _context.SavedArticles
                .FirstOrDefaultAsync(s => s.UserId == userId && s.ArticleId == articleId);

            if (savedArticle == null) return false;

            _context.SavedArticles.Remove(savedArticle);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(int userId, int articleId)
        {
            return await _context.SavedArticles
                .AnyAsync(s => s.UserId == userId && s.ArticleId == articleId);
        }
    }
}
