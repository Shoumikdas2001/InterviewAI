namespace InterviewAI.Domain.Entities;

/// <summary>
/// Marker interface to ensure all MongoDB entities have a consistent Id property.
/// </summary>
public interface IEntity
{
    string Id { get; set; }
}
