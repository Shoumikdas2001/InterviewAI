using InterviewAI.Application.DTOs.Resume;
using InterviewAI.Application.Interfaces;
using InterviewAI.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using UglyToad.PdfPig;

namespace InterviewAI.Application.Services;

public class ResumeParserService : IResumeParserService
{
    private readonly IAIProvider _aiProvider;
    private readonly ILogger<ResumeParserService> _logger;

    public ResumeParserService(IAIProvider aiProvider, ILogger<ResumeParserService> logger)
    {
        _aiProvider = aiProvider;
        _logger = logger;
    }

    public string ExtractText(byte[] pdfBytes)
    {
        try
        {
            using var document = PdfDocument.Open(pdfBytes);
            var sb = new System.Text.StringBuilder();
            foreach (var page in document.GetPages())
            {
                sb.AppendLine(page.Text);
            }
            return sb.ToString().Trim();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to extract text from PDF");
            throw new InvalidOperationException("Could not read the PDF file. Ensure it is a valid, text-based PDF.", ex);
        }
    }

    public async Task<GeminiAtsResponse?> ParseAndAnalyzeAsync(string extractedText, CancellationToken ct = default)
    {
        var prompt = $$"""
            You are an expert ATS (Applicant Tracking System) and resume analyst.
            
            Analyze the following resume text and provide a comprehensive evaluation.
            
            RESUME TEXT:
            {{extractedText}}
            
            Provide your response as a JSON object with EXACTLY this structure:
            {
              "atsScore": <integer 0-100>,
              "formattingScore": <integer 0-100>,
              "keywordScore": <integer 0-100>,
              "experienceScore": <integer 0-100>,
              "strengths": ["<strength1>", "<strength2>", ...],
              "weaknesses": ["<weakness1>", "<weakness2>", ...],
              "missingKeywords": ["<keyword1>", "<keyword2>", ...],
              "recommendations": ["<recommendation1>", "<recommendation2>", ...],
              "fullName": "<extracted full name>",
              "email": "<extracted email>",
              "phone": "<extracted phone>",
              "location": "<extracted location>",
              "summary": "<professional summary or objective>",
              "skills": ["<skill1>", "<skill2>", ...],
              "totalYearsExperience": <number>
            }
            
            Be specific and actionable in your feedback. Provide at least 3 items in each list.
            """;

        return await _aiProvider.GenerateStructuredResponseAsync<GeminiAtsResponse>(prompt, ct);
    }
}

public class ResumeService : IResumeService
{
    private readonly IResumeRepository _resumeRepository;
    private readonly IResumeParserService _parserService;
    private readonly ILogger<ResumeService> _logger;

    private const long MaxFileSizeBytes = 5 * 1024 * 1024; // 5 MB

    public ResumeService(
        IResumeRepository resumeRepository,
        IResumeParserService parserService,
        ILogger<ResumeService> logger)
    {
        _resumeRepository = resumeRepository;
        _parserService = parserService;
        _logger = logger;
    }

    public async Task<ResumeUploadResponse> UploadResumeAsync(string userId, IFormFile file, CancellationToken ct = default)
    {
        ValidateFile(file);

        using var ms = new MemoryStream();
        await file.CopyToAsync(ms, ct);
        var fileBytes = ms.ToArray();

        // Extract text immediately on upload
        var extractedText = _parserService.ExtractText(fileBytes);

        var resume = new Resume
        {
            UserId = userId,
            FileName = $"{Guid.NewGuid()}.pdf",
            OriginalFileName = file.FileName,
            FileSizeBytes = file.Length,
            ContentType = file.ContentType,
            FileData = fileBytes,
            ExtractedText = extractedText
        };

        // Deactivate previous resumes
        var existing = await _resumeRepository.GetByUserIdAsync(userId, ct);
        foreach (var old in existing)
        {
            old.IsActive = false;
            await _resumeRepository.UpdateAsync(old.Id, old, ct);
        }

        await _resumeRepository.CreateAsync(resume, ct);

        _logger.LogInformation("Resume uploaded for user {UserId}: {FileName}", userId, file.FileName);

        return new ResumeUploadResponse(
            resume.Id,
            resume.OriginalFileName,
            resume.FileSizeBytes,
            resume.UploadedAt,
            "Resume uploaded successfully. Run ATS analysis to get your score."
        );
    }

    public async Task<ResumeDto?> GetResumeAsync(string resumeId, string userId, CancellationToken ct = default)
    {
        var resume = await _resumeRepository.GetByIdAsync(resumeId, ct);
        if (resume is null || resume.UserId != userId) return null;
        return MapToDto(resume);
    }

    public async Task<IEnumerable<ResumeDto>> GetUserResumesAsync(string userId, CancellationToken ct = default)
    {
        var resumes = await _resumeRepository.GetByUserIdAsync(userId, ct);
        return resumes.Select(MapToDto);
    }

    public async Task<AtsAnalysisDto> AnalyzeResumeAsync(string resumeId, string userId, CancellationToken ct = default)
    {
        var resume = await _resumeRepository.GetByIdAsync(resumeId, ct)
            ?? throw new InvalidOperationException("Resume not found.");

        if (resume.UserId != userId)
            throw new UnauthorizedAccessException("Access denied.");

        var text = string.IsNullOrWhiteSpace(resume.ExtractedText)
            ? _parserService.ExtractText(resume.FileData ?? [])
            : resume.ExtractedText;

        var geminiResult = await _parserService.ParseAndAnalyzeAsync(text, ct)
            ?? throw new InvalidOperationException("AI analysis returned no result.");

        var analysis = new AtsAnalysis
        {
            AtsScore = geminiResult.AtsScore,
            FormattingScore = geminiResult.FormattingScore,
            KeywordScore = geminiResult.KeywordScore,
            ExperienceScore = geminiResult.ExperienceScore,
            Strengths = geminiResult.Strengths,
            Weaknesses = geminiResult.Weaknesses,
            MissingKeywords = geminiResult.MissingKeywords,
            Recommendations = geminiResult.Recommendations
        };

        var parsedContent = new ParsedResumeContent
        {
            FullName = geminiResult.FullName,
            Email = geminiResult.Email,
            Phone = geminiResult.Phone,
            Location = geminiResult.Location,
            Summary = geminiResult.Summary,
            Skills = geminiResult.Skills,
            TotalYearsExperience = geminiResult.TotalYearsExperience
        };

        await _resumeRepository.UpdateAtsAnalysisAsync(resumeId, analysis, ct);
        await _resumeRepository.UpdateParsedContentAsync(resumeId, parsedContent, ct);

        return new AtsAnalysisDto(
            analysis.AtsScore,
            analysis.FormattingScore,
            analysis.KeywordScore,
            analysis.ExperienceScore,
            analysis.Strengths,
            analysis.Weaknesses,
            analysis.MissingKeywords,
            analysis.Recommendations
        );
    }

    public async Task<byte[]> GetResumeFileAsync(string resumeId, string userId, CancellationToken ct = default)
    {
        var resume = await _resumeRepository.GetByIdAsync(resumeId, ct)
            ?? throw new InvalidOperationException("Resume not found.");

        if (resume.UserId != userId)
            throw new UnauthorizedAccessException("Access denied.");

        return resume.FileData ?? throw new InvalidOperationException("File data not available.");
    }

    public async Task DeleteResumeAsync(string resumeId, string userId, CancellationToken ct = default)
    {
        var resume = await _resumeRepository.GetByIdAsync(resumeId, ct)
            ?? throw new InvalidOperationException("Resume not found.");

        if (resume.UserId != userId)
            throw new UnauthorizedAccessException("Access denied.");

        await _resumeRepository.DeleteAsync(resumeId, ct);
    }

    private static void ValidateFile(IFormFile file)
    {
        if (file is null || file.Length == 0)
            throw new ArgumentException("No file provided.");

        if (file.Length > MaxFileSizeBytes)
            throw new ArgumentException($"File size exceeds 5MB limit.");

        if (!file.ContentType.Equals("application/pdf", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("Only PDF files are accepted.");

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (extension != ".pdf")
            throw new ArgumentException("Only .pdf files are accepted.");
    }

    private static ResumeDto MapToDto(Resume r) => new(
        r.Id,
        r.UserId,
        r.OriginalFileName,
        r.FileSizeBytes,
        r.UploadedAt,
        r.AnalyzedAt,
        r.ParsedContent,
        r.AtsAnalysis is null ? null : new AtsAnalysisDto(
            r.AtsAnalysis.AtsScore,
            r.AtsAnalysis.FormattingScore,
            r.AtsAnalysis.KeywordScore,
            r.AtsAnalysis.ExperienceScore,
            r.AtsAnalysis.Strengths,
            r.AtsAnalysis.Weaknesses,
            r.AtsAnalysis.MissingKeywords,
            r.AtsAnalysis.Recommendations
        )
    );
}
