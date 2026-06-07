using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace InterviewAI.Domain.Entities;

public class Report
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    [BsonElement("userId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string UserId { get; set; } = string.Empty;

    [BsonElement("sessionId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string SessionId { get; set; } = string.Empty;

    [BsonElement("title")]
    public string Title { get; set; } = string.Empty;

    [BsonElement("jobRole")]
    public string JobRole { get; set; } = string.Empty;

    [BsonElement("overallScore")]
    public double OverallScore { get; set; }

    [BsonElement("strengths")]
    public List<string> Strengths { get; set; } = [];

    [BsonElement("weaknesses")]
    public List<string> Weaknesses { get; set; } = [];

    [BsonElement("topicsCovered")]
    public List<string> TopicsCovered { get; set; } = [];

    [BsonElement("recommendedTopics")]
    public List<string> RecommendedTopics { get; set; } = [];

    [BsonElement("generalFeedback")]
    public string GeneralFeedback { get; set; } = string.Empty;

    [BsonElement("pdfData")]
    public byte[]? PdfData { get; set; }

    [BsonElement("generatedAt")]
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}
