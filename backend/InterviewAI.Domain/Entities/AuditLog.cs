using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace InterviewAI.Domain.Entities;

public class AuditLog
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    [BsonElement("userId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? UserId { get; set; }

    [BsonElement("action")]
    public string Action { get; set; } = string.Empty;

    [BsonElement("entity")]
    public string Entity { get; set; } = string.Empty;

    [BsonElement("entityId")]
    public string? EntityId { get; set; }

    [BsonElement("details")]
    public string? Details { get; set; }

    [BsonElement("ipAddress")]
    public string? IpAddress { get; set; }

    [BsonElement("userAgent")]
    public string? UserAgent { get; set; }

    [BsonElement("isSuccess")]
    public bool IsSuccess { get; set; } = true;

    [BsonElement("errorMessage")]
    public string? ErrorMessage { get; set; }

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
