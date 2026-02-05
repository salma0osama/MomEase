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

        public async Task<string> GenerateReplyAsync(string message)
        {
            // Validate input message
            if (string.IsNullOrWhiteSpace(message))
                throw new ArgumentException("Message cannot be empty or whitespace.", nameof(message));

            try
            {
                // Prepare request body for LLaMA API
                var body = new
                {
                    model = "llama-3.3-70b-versatile",
                    messages = new[]
                    {
                        new
                        {
                            role = "system",
                            content = "You are a helpful assistant for postpartum mothers. Provide supportive, accurate, and informative responses in Arabic or English based on the user's language. Keep responses concise and empathetic."
                        },
                        new
                        {
                            role = "user",
                            content = message
                        }
                    },
                    temperature = 0.7,
                    max_tokens = 1024,
                    top_p = 1,
                    stream = false
                };

                // Create HTTP request
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

                // Send request to LLaMA API
                var response = await _httpClient.SendAsync(request);

                // Check if request was successful
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();

                    throw new HttpRequestException(
                        $"LLaMA API returned status code {response.StatusCode}. Details: {errorContent}");
                }

                // Parse response
                var resultJson = await response.Content.ReadAsStringAsync();
                var jsonDoc = JsonSerializer.Deserialize<JsonElement>(resultJson);

                // Extract bot's reply from response
                var content = jsonDoc
                    .GetProperty("choices")[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString();

                if (string.IsNullOrWhiteSpace(content))
                    throw new InvalidOperationException("Received empty response from LLaMA API");

                return content.Trim();
            }
            catch (HttpRequestException)
            {
                // Re-throw HTTP errors to be handled by the service layer
                throw;
            }
            catch (TaskCanceledException)
            {
                // Request timeout
                throw new HttpRequestException("Request to LLaMA API timed out after 30 seconds");
            }
            catch (JsonException ex)
            {
                // JSON parsing error
                throw new InvalidOperationException("Failed to parse response from LLaMA API", ex);
            }
            catch (Exception ex)
            {
                // Any other unexpected error
                throw new InvalidOperationException($"Unexpected error while calling LLaMA API: {ex.Message}", ex);
            }
        }
    }
}
