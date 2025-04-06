using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;

namespace MindCare.Services
{
    public class ClaudeAiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _apiUrl;
        private readonly Dictionary<string, List<object>> _conversationHistory;

        public ClaudeAiService(IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("ClaudeAPI");
            _apiKey = configuration["ClaudeAI:ApiKey"];
            _apiUrl = configuration["ClaudeAI:ApiUrl"] ?? "https://api.anthropic.com/v1/messages";
            _conversationHistory = new Dictionary<string, List<object>>();

            // Set up HTTP client defaults
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<string> GetResponseAsync(string message, string userId)
        {
            try
            {
                // Initialize conversation history for this user if it doesn't exist
                if (!_conversationHistory.ContainsKey(userId))
                {
                    _conversationHistory[userId] = new List<object>();
                }

                // Add user message to history
                _conversationHistory[userId].Add(new MessageContent
                {
                    Role = "user",
                    Content = message
                });

                // Limit history to last 10 messages
                if (_conversationHistory[userId].Count > 20)
                {
                    _conversationHistory[userId].RemoveAt(0);
                }

                // Create the request payload
                var requestData = new ClaudeRequest
                {
                    Model = "claude-3-sonnet-20240229", // Fallback to a known working model
                    MaxTokens = 1000,
                    Messages = _conversationHistory[userId],
                    System = "You are MindCare's mental health chatbot assistant. Your purpose is to provide supportive responses, track user mood, offer mental health resources, relaxation techniques, and connect users with professionals when needed. Keep responses compassionate, helpful, and concise. Don't refer to yourself as Claude or Anthropic's assistant, but as MindCare Assistant."
                };

                // Serialize with correct options
                var options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                };

                var jsonContent = JsonSerializer.Serialize(requestData, options);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                // Add API key and version to headers for each request
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                _httpClient.DefaultRequestHeaders.Add("x-api-key", _apiKey);
                _httpClient.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");

                // For debugging
                Console.WriteLine($"Request body: {jsonContent}");

                // Send request to Claude API
                var response = await _httpClient.PostAsync(_apiUrl, content);
                var jsonResponse = await response.Content.ReadAsStringAsync();

                // For debugging
                Console.WriteLine($"Response status: {response.StatusCode}");
                Console.WriteLine($"Response body: {jsonResponse}");

                if (response.IsSuccessStatusCode)
                {
                    var responseObject = JsonSerializer.Deserialize<ClaudeResponse>(jsonResponse, options);

                    // Extract response text
                    var assistantResponse = responseObject?.Content?.FirstOrDefault()?.Text;

                    // Add assistant response to history
                    if (!string.IsNullOrEmpty(assistantResponse))
                    {
                        _conversationHistory[userId].Add(new MessageContent
                        {
                            Role = "assistant",
                            Content = assistantResponse
                        });
                    }

                    return assistantResponse ?? "I'm sorry, I couldn't process that response.";
                }
                else
                {
                    // Log the error details for debugging
                    Console.WriteLine($"Claude API Error: {jsonResponse}");

                    // Use a fallback response system
                    return GetFallbackResponse(message);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception when calling Claude API: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");

                // Use a fallback response system
                return GetFallbackResponse(message);
            }
        }

        private string GetFallbackResponse(string message)
        {
            // Simple fallback response system for when the API is unavailable
            message = message.ToLower();

            if (message.Contains("hello") || message.Contains("hi"))
                return "Hello! I'm your MindCare Assistant. How are you feeling today?";

            if (message.Contains("help") || message.Contains("what can you do"))
                return "I can help with mental health resources, mood tracking, relaxation techniques, and general support. What would you like help with today?";

            if (message.Contains("stress") || message.Contains("anxious") || message.Contains("anxiety"))
                return "I'm sorry to hear you're feeling stressed. Deep breathing exercises can help: try breathing in for 4 counts, holding for 4, and exhaling for 6. Would you like more stress management techniques?";

            if (message.Contains("depress") || message.Contains("sad") || message.Contains("down"))
                return "I'm here for you. Depression can be challenging, but support is available. Would you like some resources or simple activities that might help improve your mood?";

            if (message.Contains("sleep") || message.Contains("insomnia") || message.Contains("tired"))
                return "Sleep is crucial for mental health. Try establishing a regular sleep schedule, avoiding screens before bed, and creating a comfortable sleep environment. Would you like more sleep hygiene tips?";

            if (message.Contains("thank"))
                return "You're welcome! I'm here to support you on your mental health journey.";

            // Default response
            return "I'm currently experiencing some technical difficulties connecting to my knowledge base. While our team works on this, could you try rephrasing your question, or ask me about stress management, mood tracking, or relaxation techniques?";
        }
    }

    // Request and response model classes
    public class ClaudeRequest
    {
        [JsonPropertyName("model")]
        public string Model { get; set; }

        [JsonPropertyName("max_tokens")]
        public int MaxTokens { get; set; }

        [JsonPropertyName("messages")]
        public List<object> Messages { get; set; }

        [JsonPropertyName("system")]
        public string System { get; set; }
    }

    public class MessageContent
    {
        [JsonPropertyName("role")]
        public string Role { get; set; }

        [JsonPropertyName("content")]
        public string Content { get; set; }
    }

    public class ClaudeResponse
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("model")]
        public string Model { get; set; }

        [JsonPropertyName("content")]
        public List<ContentItem> Content { get; set; }
    }

    public class ContentItem
    {
        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("text")]
        public string Text { get; set; }
    }
}