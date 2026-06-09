using MomEase.core.DTOS.ChatBot;
using MomEase.core.Entities;
using MomEase.core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MomEase.infra.Services
{
    public class ChatBotService : IChatBotService
    {
        private readonly IChatBotRepository _chatRepository;
        private readonly ILlamaService _llamaService;

        public ChatBotService(
            IChatBotRepository chatRepository,
            ILlamaService llamaService)
        {
            _chatRepository = chatRepository ?? throw new ArgumentNullException(nameof(chatRepository));
            _llamaService = llamaService ?? throw new ArgumentNullException(nameof(llamaService));
        }

        public async Task<ChatResponseDto> SendMessageAsync(ChatRequestDto request)
        {
            // Validate input parameters
            if (request == null)
                throw new ArgumentNullException(nameof(request), "Request cannot be null");

            if (request.UserId <= 0)
                throw new ArgumentException("Invalid user ID. User ID must be greater than zero.", nameof(request.UserId));

            if (string.IsNullOrWhiteSpace(request.Message))
                throw new ArgumentException("Message cannot be empty or whitespace.", nameof(request.Message));

            try
            {
                Console.WriteLine($"[{DateTime.Now.AddHours(1):yyyy-MM-dd HH:mm:ss}] 📨 Message received from user {request.UserId}: {request.Message.Substring(0, Math.Min(50, request.Message.Length))}");
                // Get or create chat session for user
                var chat = await _chatRepository.GetOrCreateChatAsync(request.UserId);

                if (chat == null)
                    throw new InvalidOperationException("Failed to create or retrieve chat session");

                Console.WriteLine($"[{DateTime.Now.AddHours(1):yyyy-MM-dd HH:mm:ss}] ✅ Chat {chat.ChatId} retrieved/created");

                // Save user's message to database
                var userMessage = new ChatMessages
                {
                    ChatId = chat.ChatId,
                    Sender = "User",
                    Message = request.Message.Trim(),
                    CreatedAt = DateTime.Now.AddHours(1)
                };

                await _chatRepository.AddMessageAsync(userMessage);
                Console.WriteLine($"[{DateTime.Now.AddHours(1):yyyy-MM-dd HH:mm:ss}] ✅ User message saved");
                // Get bot's reply from LLaMA API
                string reply;
                try
                {
                    Console.WriteLine($"[{DateTime.Now.AddHours(1):yyyy-MM-dd HH:mm:ss}] 🤖 Calling LLaMA API...");
                    // Get last 6 messages for context
                    var history = await _chatRepository.GetChatHistoryAsync(chat.ChatId);

                    var lastMessages = history
                        .OrderByDescending(m => m.CreatedAt)
                        .Take(6)
                        .Reverse()
                        .Select(m => (
                            role: m.Sender == "User" ? "user" : "assistant",
                            content: m.Message
                        ))
                        .ToList();

                    // Add current user message
                    lastMessages.Add(("user", request.Message.Trim()));

                    var arabicKeywords = new[] { "نزيف شديد", "إغماء", "مش قادرة أتنفس", "أفكار انتحار", "أذى لنفسي" };
                    var englishKeywords = new[] { "suicide", "heavy bleeding", "can't breathe" };

                    if (arabicKeywords.Any(k => request.Message.Contains(k, StringComparison.OrdinalIgnoreCase)))
                    {
                        reply = @"أنا قلق جداً بشأن ما تصفينه. 
              إذا كنتِ تعانين من أعراض شديدة، يرجى طلب العناية الطبية الفورية أو الذهاب إلى أقرب غرفة طوارئ فوراً. 
              إذا كان الأمر عاجلاً، اتصلي بخدمات الطوارئ الآن. سلامتك هي أهم شيء.";
                        Console.WriteLine($"[{DateTime.Now.AddHours(1):yyyy-MM-dd HH:mm:ss}] ⚠️ Critical keywords detected (Arabic)");
                    }
                    else if (englishKeywords.Any(k => request.Message.Contains(k, StringComparison.OrdinalIgnoreCase)))
                    {
                        reply = @"I'm really concerned about what you're describing.
              If you're experiencing severe symptoms, please seek immediate medical attention or go to the nearest emergency room immediately.
              If this is urgent, call emergency services right now.
              Your safety is the most important thing.";
                        Console.WriteLine($"[{DateTime.Now.AddHours(1):yyyy-MM-dd HH:mm:ss}] ⚠️ Critical keywords detected (English)");
                    }
                    else
                    {
                        reply = await _llamaService.GenerateReplyAsync(lastMessages);
                        Console.WriteLine($"[{DateTime.Now.AddHours(1):yyyy-MM-dd HH:mm:ss}] ✅ Reply generated from LLaMA");
                    }

                    if (string.IsNullOrWhiteSpace(reply))
                        reply = "Sorry, an error occurred while processing your message. Please try again.";
                    Console.WriteLine($"[{DateTime.Now.AddHours(1):yyyy-MM-dd HH:mm:ss}] ❌ Empty reply received");
                }
                catch (HttpRequestException ex)
                {

                    // External API connection error
                    reply = "Sorry, the chatbot service is temporarily unavailable. Please try again later.";

                    // Log the error (optional: you can use ILogger)
                    Console.WriteLine($"LLaMA API Error: {ex.Message}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[{DateTime.Now.AddHours(1):yyyy-MM-dd HH:mm:ss}] ❌ UNEXPECTED ERROR: {ex.Message}");
                    Console.WriteLine($"[{DateTime.Now.AddHours(1):yyyy-MM-dd HH:mm:ss}] ❌ Stack Trace: {ex.StackTrace}");
                    // Unexpected error from LLaMA service
                    reply = "Sorry, an unexpected error occurred.";
                    Console.WriteLine($"Unexpected error in LLaMA service: {ex.Message}");
                }

                // Save bot's reply to database
                var botMessage = new ChatMessages
                {
                    ChatId = chat.ChatId,
                    Sender = "Bot",
                    Message = reply,
                    CreatedAt = DateTime.Now.AddHours(1)
                };

                await _chatRepository.AddMessageAsync(botMessage);
                Console.WriteLine($"[{DateTime.Now.AddHours(1):yyyy-MM-dd HH:mm:ss}] ✅ Bot message saved");

                return new ChatResponseDto
                {
                    Reply = reply,
                    CreatedAt = botMessage.CreatedAt
                };
            }
            catch (Exception ex) when (!(ex is ArgumentException || ex is InvalidOperationException))
            {
                // Log unexpected errors
                Console.WriteLine($"Error in SendMessageAsync: {ex.Message}");
                throw new InvalidOperationException("Failed to send message. Please try again.", ex);
            }

        }

        public async Task<ChatHistoryDto> GetChatHistoryAsync(int userId)
        {
            // Validate userId
            if (userId <= 0)
                throw new ArgumentException("Invalid user ID. User ID must be greater than zero.", nameof(userId));

            try
            {
                // Get or create chat session for user
                var chat = await _chatRepository.GetOrCreateChatAsync(userId);

                if (chat == null)
                    throw new InvalidOperationException("Failed to retrieve chat session");

                // Get all messages for this chat
                var messages = await _chatRepository.GetChatHistoryAsync(chat.ChatId);

                return new ChatHistoryDto
                {
                    ChatId = chat.ChatId,
                    Messages = messages.Select(m => new MessageDto
                    {
                        Sender = m.Sender,
                        Message = m.Message,
                        CreatedAt = m.CreatedAt
                    }).ToList()
                };
            }
            catch (Exception ex)
            {
                // Log the error
                Console.WriteLine($"Error in GetChatHistoryAsync: {ex.Message}");
                throw new InvalidOperationException("Failed to retrieve chat history. Please try again.", ex);
            }
        }
        public async Task<bool> DeleteChatAsync(int userId, int chatId)
        {
            if (userId <= 0)
                throw new ArgumentException("Invalid user ID", nameof(userId));

            if (chatId <= 0)
                throw new ArgumentException("Invalid chat ID", nameof(chatId));

            try
            {
                // ✅ تحقق من أن الـ chat ينتمي للـ user
                var chat = await _chatRepository.GetOrCreateChatAsync(userId);

                if (chat.ChatId != chatId)
                    throw new UnauthorizedAccessException("You don't have permission to delete this chat");

                // احذف الـ Chat
                var result = await _chatRepository.DeleteChatAsync(chatId);

                if (result)
                {
                    Console.WriteLine($"✅ Chat {chatId} deleted for user {userId}");
                    return true;
                }
                else
                {
                    Console.WriteLine($"⚠️ Chat {chatId} not found");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error in DeleteChatAsync: {ex.Message}");
                throw;
            }
        }
    }
}
