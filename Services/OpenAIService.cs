using System.Text;
using System.Text.Json;

namespace InfoPoint.Services
{
    public interface IOpenAIService
    {
        Task<string> EnhanceSmartTarget(string roughTarget);
    }

    public class OpenAIService : IOpenAIService
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public OpenAIService(IConfiguration configuration, HttpClient httpClient)
        {
            _configuration = configuration;
            _httpClient = httpClient;
            _apiKey = _configuration["OpenAI:ApiKey"] ?? throw new InvalidOperationException("OpenAI API key not configured");
        }

        public async Task<string> EnhanceSmartTarget(string roughTarget)
        {
            Console.WriteLine($"API Key configured: {!string.IsNullOrEmpty(_apiKey)}");
            Console.WriteLine($"API Key length: {_apiKey?.Length ?? 0}");
            
            var prompt = @"You are an HR performance coach for a UK college. 
Rewrite the provided rough goal into ONE high-quality SMART target (Specific, Measurable, Achievable, Relevant, Time-bound) in UK English.

Requirements:
- Keep it professional, concise, and action-oriented.
- Make sensible, low-risk assumptions only if necessary (do not add sensitive/personal details).
- Use precise measures and a clear deadline (date or frequency within the review period).
- Avoid jargon and fluff; keep to one short paragraph (1–3 sentences).
- Output ONLY the final SMART target text—no headings, bullets, notes, or explanations.";

            var requestBody = new
            {
                model = "gpt-4o-mini",  // Using a valid model name
                messages = new[]
                {
                    new { role = "system", content = prompt },
                    new { role = "user", content = roughTarget }
                },
                temperature = 0.7,
                max_tokens = 200
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/chat/completions");
            request.Headers.Add("Authorization", $"Bearer {_apiKey}");
            request.Content = content;

            var response = await _httpClient.SendAsync(request);
            
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"OpenAI API Response Status: {response.StatusCode}");
                Console.WriteLine($"OpenAI API Error Response: {error}");
                throw new Exception($"OpenAI API error: {response.StatusCode} - {error}");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var responseData = JsonSerializer.Deserialize<JsonElement>(responseContent);
            
            return responseData.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString() ?? roughTarget;
        }
    }
}