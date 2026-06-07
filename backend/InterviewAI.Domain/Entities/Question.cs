using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace InterviewAI.Domain.Entities;

public class Question
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    [BsonElement("sessionId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string SessionId { get; set; } = string.Empty;

    [BsonElement("userId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string UserId { get; set; } = string.Empty;

    [BsonElement("questionText")]
    public string QuestionText { get; set; } = string.Empty;

    [BsonElement("category")]
    public string Category { get; set; } = string.Empty;

    [BsonElement("skillTag")]
    public string SkillTag { get; set; } = string.Empty;

    [BsonElement("orderIndex")]
    public int OrderIndex { get; set; }

    [BsonElement("isFollowUp")]
    public bool IsFollowUp { get; set; } = false;

    [BsonElement("parentQuestionId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? ParentQuestionId { get; set; }

    [BsonElement("expectedKeyPoints")]
    public List<string> ExpectedKeyPoints { get; set; } = [];

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
