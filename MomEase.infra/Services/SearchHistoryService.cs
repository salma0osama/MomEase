using MomEase.core.DTOS.ArticlesDTOs;
using MomEase.core.Entities;
using MomEase.core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MomEase.infra.Services
{
    public class SearchHistoryService : ISearchHistoryService
    {
        private readonly ISearchHistoryRepository _repository;

        public SearchHistoryService(ISearchHistoryRepository repository)
        {
            _repository = repository;
        }

        public async Task<SearchHistoryDto> AddSearchAsync(int userId, string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                throw new Exception("Search term cannot be empty");

            // تحقق إذا كان موجود بالفعل، لا تضيفه مرة أخرى
            var exists = await _repository.ExistsAsync(userId, searchTerm);
            if (exists)
            {
                // لو موجود، نرجع null أو نتجاهله
                return null;
            }

            var searchHistory = new SearchHistory
            {
                UserId = userId,
                SearchTerm = searchTerm.Trim(),
                SearchedAt = DateTime.Now
            };

            var created = await _repository.CreateAsync(searchHistory);

            return new SearchHistoryDto
            {
                SearchId = created.SearchId,
                SearchTerm = created.SearchTerm,
                SearchedAt = created.SearchedAt
            };
        }

        public async Task<IEnumerable<SearchHistoryDto>> GetHistoryAsync(int userId)
        {
            var history = await _repository.GetByUserIdAsync(userId);

            return history.Select(h => new SearchHistoryDto
            {
                SearchId = h.SearchId,
                SearchTerm = h.SearchTerm,
                SearchedAt = h.SearchedAt
            });
        }

        public async Task<bool> ClearAllHistoryAsync(int userId)
        {
            return await _repository.DeleteAllByUserIdAsync(userId);
        }

        public async Task<bool> DeleteSearchTermAsync(int userId, string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                throw new Exception("Search term cannot be empty");

            return await _repository.DeleteBySearchTermAsync(userId, searchTerm.Trim());
        }
    }
}
