using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using InterviewAI.Domain.Enums;

namespace InterviewAI.Domain.Entities;

public class InterviewSession
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    [BsonElement("userId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string UserId { get; set; } = string.Empty;

    [BsonElement("resumeId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? ResumeId { get; set; }

    [BsonElement("title")]
    public string Title { get; set; } = string.Empty;

    [BsonElement("jobRole")]
    public string JobRole { get; set; } = string.Empty;

    [BsonElement("experienceLevel")]
    public string ExperienceLevel { get; set; } = string.Empty;

    [BsonElement("skills")]
    public List<string> Skills { get; set; } = [];

    [BsonElement("interviewType")]
    [BsonRepresentation(BsonType.String)]
    public InterviewType InterviewType { get; set; }

    [BsonElement("difficulty")]
    [BsonRepresentation(BsonType.String)]
    public DifficultyLevel Difficulty { get; set; }

    [BsonElement("status")]
    [BsonRepresentation(BsonType.String)]
    public InterviewStatus Status { get; set; } = InterviewStatus.Created;

    [BsonElement("questionIds")]
    public List<string> QuestionIds { get; set; } = [];

    [BsonElement("overallScore")]
    public double? OverallScore { get; set; }

    [BsonElement("technicalScore")]
    public double? TechnicalScore { get; set; }

    [BsonElement("communicationScore")]
    public double? CommunicationScore { get; set; }

    [BsonElement("confidenceScore")]
    public double? ConfidenceScore { get; set; }

    [BsonElement("problemSolvingScore")]
    public double? ProblemSolvingScore { get; set; }

    [BsonElement("durationMinutes")]
    public double? DurationMinutes { get; set; }

    [BsonElement("skillGapAnalysis")]
    public SkillGapAnalysis? SkillGapAnalysis { get; set; }

    [BsonElement("reportId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? ReportId { get; set; }

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("startedAt")]
    public DateTime? StartedAt { get; set; }

    [BsonElement("completedAt")]
    public DateTime? CompletedAt { get; set; }
}

public class SkillGapAnalysis
{
    [BsonElement("strengths")]
    public List<string> Strengths { get; set; } = [];

    [BsonElement("weaknesses")]
    public List<string> Weaknesses { get; set; } = [];

    [BsonElement("missingTopics")]
    public List<string> MissingTopics { get; set; } = [];

    [BsonElement("recommendations")]
    public List<string> Recommendations { get; set; } = [];

    [BsonElement("skillScores")]
    public Dictionary<string, int> SkillScores { get; set; } = [];
}
