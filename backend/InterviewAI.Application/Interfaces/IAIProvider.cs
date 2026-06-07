namespace InterviewAI.Application.Interfaces;

/// <summary>
/// AI provider abstraction. Allows swapping Gemini for OpenAI or others without changing service code.
/// </summary>
public interface IAIProvider
{
    /// <summary>
    /// Send a prompt and receive a structured JSON response parsed into type T.
    /// </summary>
    Task<T?> GenerateStructuredResponseAsync<T>(string prompt, CancellationToken ct = default) where T : class;

    /// <summary>
    /// Send a prompt and receive a raw text response.
    /// </summary>
    Task<string> GenerateTextResponseAsync(string prompt, CancellationToken ct = default);

    /// <summary>
    /// Check if the AI provider is healthy and responsive.
    /// </summary>
    Task<bool> IsHealthyAsync(CancellationToken ct = default);
}
