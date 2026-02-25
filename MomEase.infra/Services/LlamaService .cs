using Microsoft.Extensions.Configuration;
using MomEase.core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MomEase.infra.Services
{
    public class LlamaService : ILlamaService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private const string ApiUrl = "https://api.groq.com/openai/v1/chat/completions";

        public LlamaService(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _apiKey = config["Groq:ApiKey"];

            if (string.IsNullOrWhiteSpace(_apiKey))
                throw new InvalidOperationException("Groq API Key is not configured in appsettings.json");

            // Set request timeout to 30 seconds
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
        }

        public async Task<string> GenerateReplyAsync(List<(string role, string content)> messages)
        {
            if (messages == null || !messages.Any())
                throw new ArgumentException("Messages cannot be empty.");

            var systemPrompt = @"
                    You are a specialized maternal health assistant for postpartum mothers.
                    
                    General Rules:
                    - Respond in the same language as the user.
                    - Be empathetic, calm, and supportive.
                    - Keep answers natural and conversational.
                    - Do NOT use numbered sections or visible headings in your response.
                    
                    If the user sends a greeting or small talk:
                    → Respond naturally and briefly like a normal assistant.
                    
                    If the user asks a medical or postpartum-related question:
                    → Internally structure your response with:
                       • empathy
                       • possible explanation
                       • practical advice
                       • when to seek medical help (if needed)
                    → But DO NOT display section titles or numbering.
                    
                    Medical Safety:
                    - Do NOT provide diagnosis.
                    - Do NOT prescribe medication doses.
                    - If severe symptoms are mentioned, clearly advise urgent medical care.
                    ";

            var allMessages = new List<object>
    {
        new { role = "system", content = systemPrompt }
    };

            allMessages.AddRange(messages.Select(m => new
            {
                role = m.role,
                content = m.content
            }));

            var body = new
            {
                model = "llama-3.3-70b-versatile",
                messages = allMessages,
                temperature = 0.5,
                max_tokens = 600,
                top_p = 0.9,
                stream = false
            };

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(ApiUrl),
                Headers =
        {
            { "Authorization", $"Bearer {_apiKey}" }
        },
                Content = new StringContent(
                    JsonSerializer.Serialize(body),
                    Encoding.UTF8,
                    "application/json")
            };

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
                throw new HttpRequestException(await response.Content.ReadAsStringAsync());

            var resultJson = await response.Content.ReadAsStringAsync();
            var jsonDoc = JsonSerializer.Deserialize<JsonElement>(resultJson);

            return jsonDoc
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString()
                ?.Trim();
        }
    }
}