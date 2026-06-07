using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace InterviewAI.Domain.Entities;

public class Answer
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    [BsonElement("questionId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string QuestionId { get; set; } = string.Empty;

    [BsonElement("sessionId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string SessionId { get; set; } = string.Empty;

    [BsonElement("userId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string UserId { get; set; } = string.Empty;

    [BsonElement("answerText")]
    public string AnswerText { get; set; } = string.Empty;

    [BsonElement("transcript")]
    public string? Transcript { get; set; }

    [BsonElement("isVoiceAnswer")]
    public bool IsVoiceAnswer { get; set; } = false;

    [BsonElement("durationSeconds")]
    public double? DurationSeconds { get; set; }

    [BsonElement("evaluation")]
    public AnswerEvaluation? Evaluation { get; set; }

    [BsonElement("submittedAt")]
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
}

public class AnswerEvaluation
{
    [BsonElement("overallScore")]
    public int OverallScore { get; set; }

    [BsonElement("technicalScore")]
    public int TechnicalScore { get; set; }

    [BsonElement("communicationScore")]
    public int CommunicationScore { get; set; }

    [BsonElement("confidenceScore")]
    public int ConfidenceScore { get; set; }

    [BsonElement("clarityScore")]
    public int ClarityScore { get; set; }

    [BsonElement("problemSolvingScore")]
    public int ProblemSolvingScore { get; set; }

    [BsonElement("completenessScore")]
    public int CompletenessScore { get; set; }

    [BsonElement("feedback")]
    public List<string> Feedback { get; set; } = [];

    [BsonElement("improvementAreas")]
    public List<string> ImprovementAreas { get; set; } = [];

    [BsonElement("suggestedFollowUps")]
    public List<string> SuggestedFollowUps { get; set; } = [];

    [BsonElement("evaluatedAt")]
    public DateTime EvaluatedAt { get; set; } = DateTime.UtcNow;
}
