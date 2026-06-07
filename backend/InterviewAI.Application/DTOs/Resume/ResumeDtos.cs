using InterviewAI.Domain.Entities;

namespace InterviewAI.Application.DTOs.Resume;

public record ResumeUploadResponse(
    string Id,
    string FileName,
    long FileSizeBytes,
    DateTime UploadedAt,
    string Message
);

public record ResumeDto(
    string Id,
    string UserId,
    string FileName,
    long FileSizeBytes,
    DateTime UploadedAt,
    DateTime? AnalyzedAt,
    ParsedResumeContent? ParsedContent,
    AtsAnalysisDto? AtsAnalysis
);

public record AtsAnalysisDto(
    int AtsScore,
    int FormattingScore,
    int KeywordScore,
    int ExperienceScore,
    List<string> Strengths,
    List<string> Weaknesses,
    List<string> MissingKeywords,
    List<string> Recommendations
);

// Used internally by the AI response parser
public record GeminiAtsResponse(
    int AtsScore,
    int FormattingScore,
    int KeywordScore,
    int ExperienceScore,
    List<string> Strengths,
    List<string> Weaknesses,
    List<string> MissingKeywords,
    List<string> Recommendations,
    string FullName,
    string Email,
    string Phone,
    string Location,
    string Summary,
    List<string> Skills,
    double TotalYearsExperience
);
