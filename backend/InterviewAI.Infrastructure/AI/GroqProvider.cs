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
/// Groq AI provider — uses Groq's OpenAI-compatible API.
/// Free tier: 14,400 requests/day, 6000 tokens/min on llama-3.3-70b-versatile.
/// To swap back to Gemini: update DI registration in ServiceExtensions.cs.
/// </summary>
public class GroqProvider : IAIProvider
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _modelName;
    private readonly ILogger<GroqProvider> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public GroqProvider(HttpClient httpClient, IConfiguration configuration, ILogger<GroqProvider> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _apiKey = configuration["Groq:ApiKey"]
            ?? throw new InvalidOperationException("Groq API key is not configured.");
        _modelName = configuration["Groq:Model"] ?? "llama-3.3-70b-versatile";
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
            var cleaned = CleanJsonResponse(rawResponse);
            return JsonSerializer.Deserialize<T>(cleaned, JsonOptions);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse Groq JSON response. Raw: {Raw}", rawResponse);
            throw new InvalidOperationException("AI returned malformed JSON.", ex);
        }
    }

    public async Task<string> GenerateTextResponseAsync(string prompt, CancellationToken ct = default)
    {
        const string url = "https://api.groq.com/openai/v1/chat/completions";

        var requestBody = new
        {
            model = _modelName,
            messages = new[]
            {
                new { role = "user", content = prompt }
            },
            temperature = 0.7,
            max_tokens = 8192
        };

        var json = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        _httpClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _apiKey);

        _logger.LogDebug("Sending request to Groq API model: {Model}", _modelName);

        HttpResponseMessage response;
        try
        {
            response = await _httpClient.PostAsync(url, content, ct);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP request to Groq failed");
            throw new InvalidOperationException("Failed to reach Groq API.", ex);
        }

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(ct);
            _logger.LogError("Groq API returned {Status}: {Error}", response.StatusCode, error);
            throw new InvalidOperationException($"Groq API error {response.StatusCode}: {error}");
        }

        var result = await response.Content.ReadFromJsonAsync<GroqResponse>(JsonOptions, ct);
        var text = result?.Choices?.FirstOrDefault()?.Message?.Content
            ?? throw new InvalidOperationException("Groq returned an empty response.");

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
        var cleaned = Regex.Replace(raw.Trim(), @"^```(?:json)?\s*", "", RegexOptions.Multiline);
        cleaned = Regex.Replace(cleaned, @"\s*```$", "", RegexOptions.Multiline);
        return cleaned.Trim();
    }
}

// Internal DTOs for Groq API response deserialization (OpenAI-compatible format)
file class GroqResponse
{
    [JsonPropertyName("choices")]
    public List<GroqChoice>? Choices { get; set; }
}

file class GroqChoice
{
    [JsonPropertyName("message")]
    public GroqMessage? Message { get; set; }
}

file class GroqMessage
{
    [JsonPropertyName("content")]
    public string? Content { get; set; }
}
