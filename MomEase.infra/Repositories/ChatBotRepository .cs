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
    public class ChatBotRepository : IChatBotRepository
    {
        private readonly MomEaseDbContext _context;

        public ChatBotRepository(MomEaseDbContext context)
        {
            _context = context;
        }

        public async Task<ChatBot> GetOrCreateChatAsync(int userId)
        {
            var chat = await _context.ChatBots
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (chat == null)
            {
                chat = new ChatBot
                {
                    UserId = userId,
                    Created_At = DateTime.Now
                };
                _context.ChatBots.Add(chat);
                await _context.SaveChangesAsync();
            }

            return chat;
        }

        public async Task AddMessageAsync(ChatMessages message)
        {
            _context.ChatMessages.Add(message);
            await _context.SaveChangesAsync();
        }

        public async Task<List<ChatMessages>> GetChatHistoryAsync(int chatId)
        {
            return await _context.ChatMessages
                .Where(m => m.ChatId == chatId)
                .OrderBy(m => m.CreatedAt)
                .ToListAsync();
        }
    }
}
