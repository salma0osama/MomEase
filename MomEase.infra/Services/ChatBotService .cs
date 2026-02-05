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
                // Get or create chat session for user
                var chat = await _chatRepository.GetOrCreateChatAsync(request.UserId);

                if (chat == null)
                    throw new InvalidOperationException("Failed to create or retrieve chat session");

                // Save user's message to database
                var userMessage = new ChatMessages
                {
                    ChatId = chat.ChatId,
                    Sender = "User",
                    Message = request.Message.Trim(),
                    CreatedAt = DateTime.Now
                };

                await _chatRepository.AddMessageAsync(userMessage);

                // Get bot's reply from LLaMA API
                string reply;
                try
                {
                    reply = await _llamaService.GenerateReplyAsync(request.Message);

                    if (string.IsNullOrWhiteSpace(reply))
                        reply = "Sorry, an error occurred while processing your message. Please try again.";
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
                    CreatedAt = DateTime.Now
                };

                await _chatRepository.AddMessageAsync(botMessage);

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
    }
}
