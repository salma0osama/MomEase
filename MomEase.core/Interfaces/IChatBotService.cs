using MomEase.core.DTOS.ChatBot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MomEase.core.Interfaces
{
    public interface IChatBotService
    {
        Task<ChatResponseDto> SendMessageAsync(ChatRequestDto request);
        Task<ChatHistoryDto> GetChatHistoryAsync(int userId);
        Task<bool> DeleteChatAsync(int userId, int chatId);
    }
}
