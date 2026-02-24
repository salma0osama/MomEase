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
    public class SearchHistoryRepository : ISearchHistoryRepository
    {
        private readonly MomEaseDbContext _context;

        public SearchHistoryRepository(MomEaseDbContext context)
        {
            _context = context;
        }

        public async Task<SearchHistory> CreateAsync(SearchHistory searchHistory)
        {
            await _context.SearchHistories.AddAsync(searchHistory);
            await _context.SaveChangesAsync();
            return searchHistory;
        }

        public async Task<IEnumerable<SearchHistory>> GetByUserIdAsync(int userId)
        {
            return await _context.SearchHistories
                .Where(s => s.UserId == userId)
                .OrderByDescending(s => s.SearchedAt)
                .Take(10)  // آخر 10 عمليات بحث فقط
                .ToListAsync();
        }

        public async Task<bool> DeleteAllByUserIdAsync(int userId)
        {
            var histories = await _context.SearchHistories
                .Where(s => s.UserId == userId)
                .ToListAsync();

            if (!histories.Any()) return false;

            _context.SearchHistories.RemoveRange(histories);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteBySearchTermAsync(int userId, string searchTerm)
        {
            var history = await _context.SearchHistories
                .Where(s => s.UserId == userId && s.SearchTerm == searchTerm)
                .ToListAsync();

            if (!history.Any()) return false;

            _context.SearchHistories.RemoveRange(history);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(int userId, string searchTerm)
        {
            return await _context.SearchHistories
                .AnyAsync(s => s.UserId == userId && s.SearchTerm == searchTerm);
        }
    }
}
