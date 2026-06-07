using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace InterviewAI.Domain.Entities;

public class RefreshToken
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    [BsonElement("userId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string UserId { get; set; } = string.Empty;

    [BsonElement("token")]
    public string Token { get; set; } = string.Empty;

    [BsonElement("jwtId")]
    public string JwtId { get; set; } = string.Empty;

    [BsonElement("isUsed")]
    public bool IsUsed { get; set; } = false;

    [BsonElement("isRevoked")]
    public bool IsRevoked { get; set; } = false;

    [BsonElement("expiresAt")]
    public DateTime ExpiresAt { get; set; }

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("ipAddress")]
    public string? IpAddress { get; set; }
}
