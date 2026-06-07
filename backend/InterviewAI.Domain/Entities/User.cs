using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using InterviewAI.Domain.Enums;

namespace InterviewAI.Domain.Entities;

public class User
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    [BsonElement("firstName")]
    public string FirstName { get; set; } = string.Empty;

    [BsonElement("lastName")]
    public string LastName { get; set; } = string.Empty;

    [BsonElement("email")]
    public string Email { get; set; } = string.Empty;

    [BsonElement("passwordHash")]
    public string PasswordHash { get; set; } = string.Empty;

    [BsonElement("role")]
    [BsonRepresentation(BsonType.String)]
    public UserRole Role { get; set; } = UserRole.User;

    [BsonElement("isActive")]
    public bool IsActive { get; set; } = true;

    [BsonElement("profilePictureUrl")]
    public string? ProfilePictureUrl { get; set; }

    [BsonElement("passwordResetToken")]
    public string? PasswordResetToken { get; set; }

    [BsonElement("passwordResetExpiry")]
    public DateTime? PasswordResetExpiry { get; set; }

    [BsonElement("totalInterviews")]
    public int TotalInterviews { get; set; } = 0;

    [BsonElement("averageScore")]
    public double AverageScore { get; set; } = 0;

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("updatedAt")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("lastLoginAt")]
    public DateTime? LastLoginAt { get; set; }
}
