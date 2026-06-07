using InterviewAI.Domain.Enums;
using InterviewAI.Domain.Entities;

namespace InterviewAI.Application.DTOs.Interview;

// --- Requests ---
public record CreateInterviewRequest(
    string JobRole,
    string ExperienceLevel,
    List<string> Skills,
    InterviewType InterviewType,
    DifficultyLevel Difficulty,
    string? ResumeId = null
);

public record SubmitAnswerRequest(
    string QuestionId,
    string SessionId,
    string AnswerText,
    bool IsVoiceAnswer = false,
    string? Transcript = null,
    double? DurationSeconds = null
);

public record GenerateFollowUpRequest(
    string SessionId,
    string QuestionId,
    string AnswerId
);

public record GenerateRoadmapRequest(
    string? SessionId,
    string JobRole,
    List<string> WeakAreas
);

// --- Responses ---
public record InterviewSessionDto(
    string Id,
    string Title,
    string JobRole,
    string ExperienceLevel,
    List<string> Skills,
    string InterviewType,
    string Difficulty,
    string Status,
    double? OverallScore,
    double? TechnicalScore,
    double? CommunicationScore,
    double? ConfidenceScore,
    double? ProblemSolvingScore,
    double? DurationMinutes,
    DateTime CreatedAt,
    DateTime? StartedAt,
    DateTime? CompletedAt,
    SkillGapAnalysisDto? SkillGapAnalysis
);

public record QuestionDto(
    string Id,
    string SessionId,
    string QuestionText,
    string Category,
    string SkillTag,
    int OrderIndex,
    bool IsFollowUp,
    string? ParentQuestionId,
    List<string> ExpectedKeyPoints
);

public record AnswerDto(
    string Id,
    string QuestionId,
    string SessionId,
    string AnswerText,
    bool IsVoiceAnswer,
    double? DurationSeconds,
    AnswerEvaluationDto? Evaluation,
    DateTime SubmittedAt
);

public record AnswerEvaluationDto(
    int OverallScore,
    int TechnicalScore,
    int CommunicationScore,
    int ConfidenceScore,
    int ClarityScore,
    int ProblemSolvingScore,
    int CompletenessScore,
    List<string> Feedback,
    List<string> ImprovementAreas,
    List<string> SuggestedFollowUps
);

public record SkillGapAnalysisDto(
    List<string> Strengths,
    List<string> Weaknesses,
    List<string> MissingTopics,
    List<string> Recommendations,
    Dictionary<string, int> SkillScores
);

public record StudyPlanDto(
    string Id,
    string Title,
    string JobRole,
    List<string> TargetSkills,
    int EstimatedHours,
    List<StudyWeekDto> Weeks,
    DateTime CreatedAt
);

public record StudyWeekDto(
    int WeekNumber,
    string Theme,
    List<StudyTopicDto> Topics,
    List<string> Resources,
    List<string> Exercises,
    int EstimatedHours
);

public record StudyTopicDto(
    string Name,
    string Description,
    string Priority
);

public record InterviewWithQuestionsDto(
    InterviewSessionDto Session,
    List<QuestionDto> Questions
);

// Gemini internal response DTOs
public record GeminiQuestionsResponse(
    List<GeminiQuestion> Questions
);

public record GeminiQuestion(
    string QuestionText,
    string Category,
    string SkillTag,
    List<string> ExpectedKeyPoints
);

public record GeminiEvaluationResponse(
    int OverallScore,
    int TechnicalScore,
    int CommunicationScore,
    int ConfidenceScore,
    int ClarityScore,
    int ProblemSolvingScore,
    int CompletenessScore,
    List<string> Feedback,
    List<string> ImprovementAreas,
    List<string> SuggestedFollowUps
);

public record GeminiSkillGapResponse(
    List<string> Strengths,
    List<string> Weaknesses,
    List<string> MissingTopics,
    List<string> Recommendations,
    Dictionary<string, int> SkillScores
);

public record GeminiRoadmapResponse(
    string Title,
    int EstimatedHours,
    List<GeminiWeek> Weeks
);

public record GeminiWeek(
    int WeekNumber,
    string Theme,
    List<GeminiTopic> Topics,
    List<string> Resources,
    List<string> Exercises,
    int EstimatedHours
);

public record GeminiTopic(
    string Name,
    string Description,
    string Priority
);
