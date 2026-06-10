using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MomEase.core.DTOS.ChatBot;
using MomEase.core.Entities;
using MomEase.core.Interfaces;
using MomEase.infra.Helpers;
using System.Text.Json;

namespace MomEase.infra.Services
{
    public class ChatBotService : IChatBotService
    {
        private readonly IChatBotRepository _chatBotRepository;
        private readonly HttpClient _httpClient;
        private readonly ILogger<ChatBotService> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly string _baseUrl;
        private readonly string _modelName;
        private readonly int _maxTokens;
        private readonly string _apiKey;

        public ChatBotService(
            IChatBotRepository chatBotRepository,
            HttpClient httpClient,
            ILogger<ChatBotService> logger,
            IHttpContextAccessor httpContextAccessor,
            IConfiguration config)
        {
            _chatBotRepository = chatBotRepository ?? throw new ArgumentNullException(nameof(chatBotRepository));
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpContextAccessor = httpContextAccessor;

            _baseUrl = config["Groq:BaseUrl"] ?? "https://api.groq.com/openai/v1";
            _modelName = config["Groq:Model"] ?? "llama3-8b-8192";
            _maxTokens = int.Parse(config["Groq:MaxTokens"] ?? "512");
            _apiKey = config["Groq:ApiKey"];

            if (string.IsNullOrWhiteSpace(_apiKey))
                throw new InvalidOperationException("Groq API Key is not configured in appsettings.json");

            _httpClient.Timeout = TimeSpan.FromSeconds(30);

            _logger.LogInformation($"🤖 ChatBot initialized with Groq at {_baseUrl}, Model: {_modelName}");
        }

        public async Task<ChatResponseDto> SendMessageAsync(ChatRequestDto request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request), "Request cannot be null");

            if (request.UserId <= 0)
                throw new ArgumentException("Invalid user ID. User ID must be greater than zero.", nameof(request.UserId));

            if (string.IsNullOrWhiteSpace(request.Message))
                throw new ArgumentException("Message cannot be empty or whitespace.", nameof(request.Message));

            try
            {
                var messageLanguage = DetectLanguage(request.Message);
                var isAr = messageLanguage == "ar";

                _logger.LogInformation($"📨 Message from user {request.UserId}: {request.Message.Substring(0, Math.Min(50, request.Message.Length))}");

                var chat = await _chatBotRepository.GetOrCreateChatAsync(request.UserId);

                if (chat == null)
                    throw new InvalidOperationException("Failed to create or retrieve chat session");

                _logger.LogInformation($"✅ Chat {chat.ChatId} retrieved/created");

                var userMessage = new ChatMessages
                {
                    ChatId = chat.ChatId,
                    Sender = "User",
                    Message = request.Message.Trim(),
                    CreatedAt = DateTime.Now
                };

                await _chatBotRepository.AddMessageAsync(userMessage);
                _logger.LogInformation($"✅ User message saved");

                string reply;
                try
                {
                    _logger.LogInformation($"🤖 Calling Groq API...");

                    if (CheckCriticalKeywords(request.Message, isAr))
                    {
                        reply = isAr
                            ? "⚠️ أنا قلق جداً بشأن ما تصفينه. إذا كان الأمر عاجلاً، يرجى طلب العناية الطبية الفورية أو الذهاب إلى أقرب غرفة طوارئ فوراً. سلامتك هي أهم شيء."
                            : "⚠️ I'm really concerned about what you're describing. If this is urgent, please seek immediate medical attention or go to the nearest emergency room right away. Your safety is the most important thing.";

                        _logger.LogWarning($"⚠️ Critical keywords detected from user {request.UserId}");
                    }
                    else
                    {
                        var history = await _chatBotRepository.GetChatHistoryAsync(chat.ChatId);
                        var lastMessages = history
                            .OrderByDescending(m => m.CreatedAt)
                            .Take(6)
                            .Reverse()
                            .Select(m => (
                                role: m.Sender == "User" ? "user" : "assistant",
                                content: m.Message
                            ))
                            .ToList();

                        lastMessages.Add(("user", request.Message.Trim()));

                        reply = await GetGroqReplyAsync(lastMessages, isAr);
                    }

                    if (string.IsNullOrWhiteSpace(reply))
                    {
                        reply = isAr
                            ? "معاذير، حدث خطأ. حاولي مرة أخرى."
                            : "Sorry, an error occurred. Please try again.";

                        _logger.LogWarning($"⚠️ Empty reply received from Groq");
                    }
                }
                catch (HttpRequestException ex)
                {
                    _logger.LogError(ex, "❌ Groq connection error");
                    reply = isAr
                        ? "معاذير، خدمة الدردشة غير متاحة حالياً. حاولي مرة أخرى لاحقاً."
                        : "Sorry, the chatbot service is temporarily unavailable. Please try again later.";
                }
                catch (TaskCanceledException ex)
                {
                    _logger.LogError(ex, "❌ Groq request timeout");
                    reply = isAr
                        ? "طلب الرد استغرق وقتاً طويلاً. حاولي مرة أخرى."
                        : "The response took too long. Please try again.";
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Unexpected error in Groq service");
                    reply = isAr
                        ? "حدث خطأ غير متوقع. حاولي مرة أخرى."
                        : "An unexpected error occurred. Please try again.";
                }

                var botMessage = new ChatMessages
                {
                    ChatId = chat.ChatId,
                    Sender = "Bot",
                    Message = reply,
                    CreatedAt = DateTime.Now
                };

                await _chatBotRepository.AddMessageAsync(botMessage);
                _logger.LogInformation($"✅ Bot message saved");

                return new ChatResponseDto
                {
                    Reply = reply,
                    CreatedAt = botMessage.CreatedAt
                };
            }
            catch (Exception ex) when (!(ex is ArgumentException || ex is InvalidOperationException))
            {
                _logger.LogError(ex, "❌ Error in SendMessageAsync");
                throw new InvalidOperationException("Failed to send message. Please try again.", ex);
            }
        }

        private async Task<string> GetGroqReplyAsync(List<(string role, string content)> messages, bool isAr)
        {
            try
            {
                var systemPrompt = isAr
                        ? @"أنتِ مساعدة صحية متخصصة للأمهات بعد الولادة، لديك معرفة عميقة بالرعاية الصحية النسائية والعناية بالأطفال الحديثي الولادة.
                    
                    STRICT RULES:
                    1. يجب الرد بالعربية الفصحى فقط
                    2. تجنبي الكليشيهات والعبارات المكررة (مثل 'أفهم قلقك')
                    3. اسألي أسئلة متابعة محددة عن الأعراض
                    4. قدمي خطوات عملية مفصلة وواضحة
                    5. ركزي على الحلول الفعلية وليس التعاطف الفارغ
                    6. اذكري الأرقام والمدد الزمنية المحددة
                    7. لا تعطي تشخيصات طبية - فقط معلومات عامة
                    8. ردي بـ 4-6 جمل فقط، مركزة وعملية
                    9. قولي 'استشيري الطبيب فوراً' فقط لـ: نزيف شديد، فقدان وعي، أفكار إيذاء النفس
                    10. استخدمي لغة طبيعية ومختلفة في كل رد - لا تكرري الجمل الافتتاحية"
                        : @"You are a specialized postpartum health assistant with deep knowledge of women's health and newborn care. You provide evidence-based advice grounded in medical research and clinical practice.
                    
                    STRICT RULES:
                    1. Respond in English ONLY - no Arabic words
                    2. Avoid clichés and repetitive phrases (never open with 'I understand your concern')
                    3. Ask specific follow-up questions about symptoms
                    4. Give detailed, actionable steps with exact measurements/timings
                    5. Focus on practical solutions over emotional validation
                    6. Include specific numbers, durations, and medical parameters
                    7. Do NOT provide medical diagnoses - give general health information only
                    8. Keep responses to 4-6 sentences, focused and practical
                    9. Only say 'seek immediate medical care' for: severe bleeding, fainting, self-harm thoughts
                    10. Use varied, natural language in each response - different opening phrases
                    11. When discussing breastfeeding, mention: latch position, pain scale (0-10), duration
                    12. For newborn issues, include: normal ranges, feeding schedules, warning signs";

                var requestBody = new
                {
                    model = _modelName,
                    messages = new[] { new { role = "system", content = systemPrompt } }
                        .Concat(messages.Select(m => new { role = m.role, content = m.content }))
                        .ToArray(),
                    temperature = 0.7,
                    max_tokens = _maxTokens,
                    top_p = 0.9
                };

                _logger.LogInformation($"📤 Sending request to Groq with {messages.Count} messages");

                var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"{_baseUrl}/chat/completions")
                {
                    Content = new StringContent(
                        JsonSerializer.Serialize(requestBody),
                        System.Text.Encoding.UTF8,
                        "application/json")
                };

                httpRequest.Headers.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _apiKey);

                var response = await _httpClient.SendAsync(httpRequest);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"❌ Groq error: {response.StatusCode} - {errorContent}");
                    throw new HttpRequestException($"Groq returned {response.StatusCode}");
                }

                var jsonResponse = await response.Content.ReadAsStringAsync();
                var jsonDoc = JsonSerializer.Deserialize<JsonElement>(jsonResponse);

                var reply = jsonDoc
                    .GetProperty("choices")[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString()?.Trim();

                if (string.IsNullOrWhiteSpace(reply))
                    throw new InvalidOperationException("Empty response from Groq");

                _logger.LogInformation($"✅ Reply received from Groq ({reply.Length} chars)");
                return reply;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error in GetGroqReplyAsync");
                throw;
            }
        }

        private bool CheckCriticalKeywords(string message, bool isAr)
        {
            var criticalAr = new[]
            {
                "نزيف شديد",
                "إغماء",
                "مش قادرة أتنفس",
                "أفكار انتحار",
                "أؤذي نفسي",
                "حالة طوارئ"
            };

            var criticalEn = new[]
            {
                "severe bleeding",
                "unconscious",
                "can't breathe",
                "suicide",
                "self harm",
                "emergency"
            };

            var keywords = isAr ? criticalAr : criticalEn;
            return keywords.Any(k => message.Contains(k, StringComparison.OrdinalIgnoreCase));
        }

        private string DetectLanguage(string text)
        {
            var arabicChars = new[] { 'ا', 'ب', 'ت', 'ث', 'ج', 'ح', 'خ', 'د', 'ذ', 'ر', 'ز', 'س', 'ش', 'ص', 'ض', 'ط', 'ظ', 'ع', 'غ', 'ف', 'ق', 'ك', 'ل', 'م', 'ن', 'ه', 'و', 'ي', 'ة' };
            var arabicCount = text.Count(c => arabicChars.Contains(c));
            var englishCount = text.Count(c => char.IsLetter(c) && !arabicChars.Contains(c));
            return arabicCount > englishCount ? "ar" : "en";
        }

        public async Task<ChatHistoryDto> GetChatHistoryAsync(int userId)
        {
            if (userId <= 0)
                throw new ArgumentException("Invalid user ID", nameof(userId));

            try
            {
                _logger.LogInformation($"📖 Fetching chat history for user {userId}");

                var chat = await _chatBotRepository.GetOrCreateChatAsync(userId);

                if (chat == null)
                    throw new InvalidOperationException("Failed to retrieve chat session");

                var messages = await _chatBotRepository.GetChatHistoryAsync(chat.ChatId);

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
                _logger.LogError(ex, "❌ Error in GetChatHistoryAsync");
                throw new InvalidOperationException("Failed to retrieve chat history. Please try again.", ex);
            }
        }

        public async Task<bool> DeleteChatAsync()
        {
            try
            {
                // ✅ اجلب userId من التوكن
                var userIdClaim = _httpContextAccessor.HttpContext?.User
                    .FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                    throw new ArgumentException("Invalid or missing user ID in token");

                _logger.LogInformation($"🗑️ Deleting chat for user {userId}");

                var chat = await _chatBotRepository.GetOrCreateChatAsync(userId);

                if (chat == null)
                    return false;

                var result = await _chatBotRepository.DeleteChatAsync(chat.ChatId);

                if (result)
                {
                    _logger.LogInformation($"✅ Chat {chat.ChatId} deleted successfully");
                    return true;
                }
                else
                {
                    _logger.LogWarning($"⚠️ Chat {chat.ChatId} not found");
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error in DeleteChatAsync");
                throw;
            }
        }
    }
}