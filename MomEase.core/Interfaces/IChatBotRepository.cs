using MomEase.core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MomEase.core.Interfaces
{
    public interface IChatBotRepository
    {
        Task<ChatBot> GetOrCreateChatAsync(int userId);
        Task AddMessageAsync(ChatMessages message);
        Task<List<ChatMessages>> GetChatHistoryAsync(int chatId);
    }
}
