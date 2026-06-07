using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using InterviewAI.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace InterviewAI.Infrastructure.AI;

/// <summary>
/// Google Gemini AI provider implementation.
/// Calls Gemini 1.5 Flash API via HTTP.
/// To swap to OpenAI: implement IAIProvider and update DI registration.
/// </summary>
public class GeminiProvider : IAIProvider
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _modelName;
    private readonly ILogger<GeminiProvider> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public GeminiProvider(HttpClient httpClient, IConfiguration configuration, ILogger<GeminiProvider> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _apiKey = configuration["Gemini:ApiKey"]
            ?? throw new InvalidOperationException("Gemini API key is not configured.");
        _modelName = configuration["Gemini:Model"] ?? "gemini-1.5-flash";
    }

    public async Task<T?> GenerateStructuredResponseAsync<T>(string prompt, CancellationToken ct = default) where T : class
    {
        var jsonPrompt = $"""
            {prompt}
            
            IMPORTANT: Respond ONLY with valid JSON. No markdown, no code blocks, no explanation.
            The response must be valid JSON that can be parsed directly.
            """;

        var rawResponse = await GenerateTextResponseAsync(jsonPrompt, ct);

        try
        {
            // Strip any markdown code blocks if present
            var cleaned = CleanJsonResponse(rawResponse);
            return JsonSerializer.Deserialize<T>(cleaned, JsonOptions);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse Gemini JSON response. Raw: {Raw}", rawResponse);
            throw new InvalidOperationException("AI returned malformed JSON.", ex);
        }
    }

    public async Task<string> GenerateTextResponseAsync(string prompt, CancellationToken ct = default)
    {
        var url = $"https://generativelanguage.googleapis.com/v1beta/models/{_modelName}:generateContent?key={_apiKey}";

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
                topK = 40,
                topP = 0.95,
                maxOutputTokens = 8192,
                responseMimeType = "text/plain"
            },
            safetySettings = new[]
            {
                new { category = "HARM_CATEGORY_HARASSMENT", threshold = "BLOCK_ONLY_HIGH" },
                new { category = "HARM_CATEGORY_HATE_SPEECH", threshold = "BLOCK_ONLY_HIGH" },
                new { category = "HARM_CATEGORY_SEXUALLY_EXPLICIT", threshold = "BLOCK_ONLY_HIGH" },
                new { category = "HARM_CATEGORY_DANGEROUS_CONTENT", threshold = "BLOCK_ONLY_HIGH" }
            }
        };

        var json = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        _logger.LogDebug("Sending request to Gemini API model: {Model}", _modelName);

        HttpResponseMessage response;
        try
        {
            response = await _httpClient.PostAsync(url, content, ct);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP request to Gemini failed");
            throw new InvalidOperationException("Failed to reach Gemini API.", ex);
        }

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(ct);
            _logger.LogError("Gemini API returned {Status}: {Error}", response.StatusCode, error);
            throw new InvalidOperationException($"Gemini API error {response.StatusCode}: {error}");
        }

        var result = await response.Content.ReadFromJsonAsync<GeminiResponse>(JsonOptions, ct);
        var text = result?.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text
            ?? throw new InvalidOperationException("Gemini returned an empty response.");

        return text;
    }

    public async Task<bool> IsHealthyAsync(CancellationToken ct = default)
    {
        try
        {
            var response = await GenerateTextResponseAsync("Say 'OK' in one word.", ct);
            return !string.IsNullOrWhiteSpace(response);
        }
        catch
        {
            return false;
        }
    }

    private static string CleanJsonResponse(string raw)
    {
        // Remove markdown code fences
        var cleaned = Regex.Replace(raw.Trim(), @"^```(?:json)?\s*", "", RegexOptions.Multiline);
        cleaned = Regex.Replace(cleaned, @"\s*```$", "", RegexOptions.Multiline);
        return cleaned.Trim();
    }
}

// Internal DTOs for Gemini API response deserialization
file class GeminiResponse
{
    [JsonPropertyName("candidates")]
    public List<GeminiCandidate>? Candidates { get; set; }
}

file class GeminiCandidate
{
    [JsonPropertyName("content")]
    public GeminiContent? Content { get; set; }
}

file class GeminiContent
{
    [JsonPropertyName("parts")]
    public List<GeminiPart>? Parts { get; set; }
}

file class GeminiPart
{
    [JsonPropertyName("text")]
    public string? Text { get; set; }
}
