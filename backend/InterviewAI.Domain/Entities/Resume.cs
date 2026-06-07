using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace InterviewAI.Domain.Entities;

public class Resume
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    [BsonElement("userId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string UserId { get; set; } = string.Empty;

    [BsonElement("fileName")]
    public string FileName { get; set; } = string.Empty;

    [BsonElement("originalFileName")]
    public string OriginalFileName { get; set; } = string.Empty;

    [BsonElement("fileSizeBytes")]
    public long FileSizeBytes { get; set; }

    [BsonElement("contentType")]
    public string ContentType { get; set; } = "application/pdf";

    [BsonElement("fileData")]
    public byte[]? FileData { get; set; }

    [BsonElement("extractedText")]
    public string ExtractedText { get; set; } = string.Empty;

    [BsonElement("parsedContent")]
    public ParsedResumeContent? ParsedContent { get; set; }

    [BsonElement("atsAnalysis")]
    public AtsAnalysis? AtsAnalysis { get; set; }

    [BsonElement("isActive")]
    public bool IsActive { get; set; } = true;

    [BsonElement("uploadedAt")]
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("analyzedAt")]
    public DateTime? AnalyzedAt { get; set; }
}

public class ParsedResumeContent
{
    [BsonElement("fullName")]
    public string FullName { get; set; } = string.Empty;

    [BsonElement("email")]
    public string Email { get; set; } = string.Empty;

    [BsonElement("phone")]
    public string Phone { get; set; } = string.Empty;

    [BsonElement("location")]
    public string Location { get; set; } = string.Empty;

    [BsonElement("summary")]
    public string Summary { get; set; } = string.Empty;

    [BsonElement("skills")]
    public List<string> Skills { get; set; } = [];

    [BsonElement("workExperience")]
    public List<WorkExperience> WorkExperience { get; set; } = [];

    [BsonElement("education")]
    public List<Education> Education { get; set; } = [];

    [BsonElement("certifications")]
    public List<string> Certifications { get; set; } = [];

    [BsonElement("projects")]
    public List<string> Projects { get; set; } = [];

    [BsonElement("languages")]
    public List<string> Languages { get; set; } = [];

    [BsonElement("totalYearsExperience")]
    public double TotalYearsExperience { get; set; }
}

public class WorkExperience
{
    [BsonElement("company")]
    public string Company { get; set; } = string.Empty;

    [BsonElement("title")]
    public string Title { get; set; } = string.Empty;

    [BsonElement("startDate")]
    public string StartDate { get; set; } = string.Empty;

    [BsonElement("endDate")]
    public string EndDate { get; set; } = string.Empty;

    [BsonElement("description")]
    public string Description { get; set; } = string.Empty;
}

public class Education
{
    [BsonElement("institution")]
    public string Institution { get; set; } = string.Empty;

    [BsonElement("degree")]
    public string Degree { get; set; } = string.Empty;

    [BsonElement("field")]
    public string Field { get; set; } = string.Empty;

    [BsonElement("graduationYear")]
    public string GraduationYear { get; set; } = string.Empty;
}

public class AtsAnalysis
{
    [BsonElement("atsScore")]
    public int AtsScore { get; set; }

    [BsonElement("strengths")]
    public List<string> Strengths { get; set; } = [];

    [BsonElement("weaknesses")]
    public List<string> Weaknesses { get; set; } = [];

    [BsonElement("missingKeywords")]
    public List<string> MissingKeywords { get; set; } = [];

    [BsonElement("recommendations")]
    public List<string> Recommendations { get; set; } = [];

    [BsonElement("formattingScore")]
    public int FormattingScore { get; set; }

    [BsonElement("keywordScore")]
    public int KeywordScore { get; set; }

    [BsonElement("experienceScore")]
    public int ExperienceScore { get; set; }
}
