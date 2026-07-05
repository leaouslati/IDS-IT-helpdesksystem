using backend.Application.Common;
using backend.Application.Interfaces;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;

namespace backend.Infrastructure.Services
{
    public class GroqService : IGroqService
    {
        private const string SystemPrompt =
            "You are an IT help desk assistant. Your job is to analyze support ticket descriptions " +
            "and suggest the most appropriate category and priority. You must respond ONLY with a valid " +
            "JSON object, no explanation, no markdown, no backticks. The response must be exactly this shape: " +
            "{\"category\": \"string\", \"priority\": \"string\", \"categoryConfidence\": number, \"priorityConfidence\": number}";

        private static readonly string[] ValidPriorities = { "Low", "Medium", "High", "Critical" };

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILookupRepository _lookupRepository;
        private readonly GroqSettings _settings;
        private readonly ILogger<GroqService> _logger;

        public GroqService(
            IHttpClientFactory httpClientFactory,
            ILookupRepository lookupRepository,
            IOptions<GroqSettings> settings,
            ILogger<GroqService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _lookupRepository  = lookupRepository;
            _settings           = settings.Value;
            _logger             = logger;
        }

        public async Task<TicketAnalysisResult> AnalyzeTicketAsync(string description)
        {
            var empty = new TicketAnalysisResult
            {
                SuggestedCategory  = null,
                SuggestedPriority  = null,
                CategoryConfidence = 0f,
                PriorityConfidence = 0f
            };

            try
            {
                var categories = (await _lookupRepository.GetCategoriesAsync())
                    .Select(c => c.Name)
                    .ToList();

                var userPrompt =
                    $"Analyze this IT support ticket and suggest category and priority.\n\n" +
                    $"Description: {description}\n\n" +
                    $"Valid categories: {string.Join(", ", categories)}\n" +
                    $"Valid priorities: {string.Join(", ", ValidPriorities)}\n\n" +
                    "Respond with JSON only.";

                var requestBody = new
                {
                    model = _settings.Model,
                    messages = new object[]
                    {
                        new { role = "system", content = SystemPrompt },
                        new { role = "user",   content = userPrompt }
                    },
                    temperature = 0.1,
                    max_tokens  = 150
                };

                var client = _httpClientFactory.CreateClient("groq");
                using var content = new StringContent(
                    JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

                using var response = await client.PostAsync("chat/completions", content);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Groq API returned {StatusCode} for ticket analysis", response.StatusCode);
                    return empty;
                }

                var responseJson = await response.Content.ReadAsStringAsync();
                using var responseDoc = JsonDocument.Parse(responseJson);
                var messageContent = responseDoc.RootElement
                    .GetProperty("choices")[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString();

                if (string.IsNullOrWhiteSpace(messageContent))
                {
                    _logger.LogWarning("Groq API returned empty content for ticket analysis");
                    return empty;
                }

                using var analysisDoc = JsonDocument.Parse(messageContent);
                var root = analysisDoc.RootElement;

                var rawCategory = root.TryGetProperty("category", out var catEl) ? catEl.GetString() : null;
                var rawPriority = root.TryGetProperty("priority", out var priEl) ? priEl.GetString() : null;
                var categoryConfidence = root.TryGetProperty("categoryConfidence", out var catConfEl) && catConfEl.TryGetSingle(out var cc) ? cc : 0f;
                var priorityConfidence = root.TryGetProperty("priorityConfidence", out var priConfEl) && priConfEl.TryGetSingle(out var pc) ? pc : 0f;

                var matchedCategory = categories.FirstOrDefault(c =>
                    string.Equals(c, rawCategory, StringComparison.OrdinalIgnoreCase));
                var matchedPriority = ValidPriorities.FirstOrDefault(p =>
                    string.Equals(p, rawPriority, StringComparison.OrdinalIgnoreCase));

                return new TicketAnalysisResult
                {
                    SuggestedCategory  = matchedCategory,
                    SuggestedPriority  = matchedPriority,
                    CategoryConfidence = matchedCategory != null ? categoryConfidence : 0f,
                    PriorityConfidence = matchedPriority != null ? priorityConfidence : 0f
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to analyze ticket description via Groq");
                return empty;
            }
        }
    }
}
