using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace InterviewAI.Domain.Entities;

public class StudyPlan
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    [BsonElement("userId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string UserId { get; set; } = string.Empty;

    [BsonElement("sessionId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? SessionId { get; set; }

    [BsonElement("title")]
    public string Title { get; set; } = string.Empty;

    [BsonElement("jobRole")]
    public string JobRole { get; set; } = string.Empty;

    [BsonElement("targetSkills")]
    public List<string> TargetSkills { get; set; } = [];

    [BsonElement("weeks")]
    public List<StudyWeek> Weeks { get; set; } = [];

    [BsonElement("estimatedHours")]
    public int EstimatedHours { get; set; }

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("updatedAt")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public class StudyWeek
{
    [BsonElement("weekNumber")]
    public int WeekNumber { get; set; }

    [BsonElement("theme")]
    public string Theme { get; set; } = string.Empty;

    [BsonElement("topics")]
    public List<StudyTopic> Topics { get; set; } = [];

    [BsonElement("resources")]
    public List<string> Resources { get; set; } = [];

    [BsonElement("exercises")]
    public List<string> Exercises { get; set; } = [];

    [BsonElement("estimatedHours")]
    public int EstimatedHours { get; set; }
}

public class StudyTopic
{
    [BsonElement("name")]
    public string Name { get; set; } = string.Empty;

    [BsonElement("description")]
    public string Description { get; set; } = string.Empty;

    [BsonElement("priority")]
    public string Priority { get; set; } = "Medium";
}
