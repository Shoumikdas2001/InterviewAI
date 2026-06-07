using InterviewAI.Application.DTOs.Resume;
using Microsoft.AspNetCore.Http;

namespace InterviewAI.Application.Interfaces;

public interface IResumeService
{
    Task<ResumeUploadResponse> UploadResumeAsync(string userId, IFormFile file, CancellationToken ct = default);
    Task<ResumeDto?> GetResumeAsync(string resumeId, string userId, CancellationToken ct = default);
    Task<IEnumerable<ResumeDto>> GetUserResumesAsync(string userId, CancellationToken ct = default);
    Task<AtsAnalysisDto> AnalyzeResumeAsync(string resumeId, string userId, CancellationToken ct = default);
    Task<byte[]> GetResumeFileAsync(string resumeId, string userId, CancellationToken ct = default);
    Task DeleteResumeAsync(string resumeId, string userId, CancellationToken ct = default);
}

public interface IResumeParserService
{
    string ExtractText(byte[] pdfBytes);
    Task<GeminiAtsResponse?> ParseAndAnalyzeAsync(string extractedText, CancellationToken ct = default);
}
