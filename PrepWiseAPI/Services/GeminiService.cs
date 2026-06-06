using System.Text;
using System.Text.Json;

namespace PrepWiseAPI.Services
{
    public class GeminiService
    {
        private readonly IConfiguration _config;
        private readonly HttpClient _httpClient;

        public GeminiService(IConfiguration config, HttpClient httpClient)
        {
            _config = config;
            _httpClient = httpClient;
        }

        public async Task<string> GenerateContentAsync(string prompt)
        {
            var apiKey = _config["GeminiSettings:ApiKey"]!;
            var model = _config["GeminiSettings:Model"] ?? "gemini-2.0-flash";

            var url = $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={apiKey}";

            var requestBody = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new { text = prompt }
                        }
                    }
                },
                generationConfig = new
                {
                    temperature = 0.7,
                    maxOutputTokens = 2048
                }
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(url, content);
            var responseText = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Gemini API error: {response.StatusCode} - {responseText}");
            }

            // Parse the Gemini response to extract the text
            using var doc = JsonDocument.Parse(responseText);
            var text = doc.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString();

            return text ?? throw new Exception("Gemini returned empty response.");
        }
    }
}
