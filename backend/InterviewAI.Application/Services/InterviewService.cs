using InterviewAI.Application.DTOs.Interview;
using InterviewAI.Application.Interfaces;
using InterviewAI.Domain.Entities;
using InterviewAI.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace InterviewAI.Application.Services;

public class InterviewService : IInterviewService
{
    private readonly IInterviewSessionRepository _sessionRepo;
    private readonly IQuestionService _questionService;
    private readonly IResumeRepository _resumeRepo;
    private readonly ILogger<InterviewService> _logger;

    public InterviewService(
        IInterviewSessionRepository sessionRepo,
        IQuestionService questionService,
        IResumeRepository resumeRepo,
        ILogger<InterviewService> logger)
    {
        _sessionRepo = sessionRepo;
        _questionService = questionService;
        _resumeRepo = resumeRepo;
        _logger = logger;
    }

    public async Task<InterviewWithQuestionsDto> CreateInterviewAsync(
        string userId, CreateInterviewRequest request, CancellationToken ct = default)
    {
        var title = $"{request.JobRole} — {request.InterviewType} ({DateTime.UtcNow:MMM d, yyyy})";

        var session = new InterviewSession
        {
            UserId = userId,
            ResumeId = request.ResumeId,
            Title = title,
            JobRole = request.JobRole,
            ExperienceLevel = request.ExperienceLevel,
            Skills = request.Skills,
            InterviewType = request.InterviewType,
            Difficulty = request.Difficulty,
            Status = InterviewStatus.Created
        };

        await _sessionRepo.CreateAsync(session, ct);

        // Generate questions immediately
        var questions = await _questionService.GenerateQuestionsAsync(session.Id, userId, ct);

        // Update session with question IDs
        session.QuestionIds = questions.Select(q => q.Id).ToList();
        await _sessionRepo.UpdateAsync(session.Id, session, ct);

        _logger.LogInformation("Interview created for user {UserId}: {SessionId}", userId, session.Id);

        return new InterviewWithQuestionsDto(MapToDto(session), questions);
    }

    public async Task<InterviewSessionDto?> GetSessionAsync(string sessionId, string userId, CancellationToken ct = default)
    {
        var session = await _sessionRepo.GetByIdAsync(sessionId, ct);
        if (session is null || session.UserId != userId) return null;
        return MapToDto(session);
    }

    public async Task<IEnumerable<InterviewSessionDto>> GetHistoryAsync(
        string userId, int page, int pageSize, CancellationToken ct = default)
    {
        var sessions = await _sessionRepo.GetByUserIdAsync(userId, page, pageSize, ct);
        return sessions.Select(MapToDto);
    }

    public async Task StartSessionAsync(string sessionId, string userId, CancellationToken ct = default)
    {
        var session = await _sessionRepo.GetByIdAsync(sessionId, ct)
            ?? throw new InvalidOperationException("Session not found.");

        if (session.UserId != userId) throw new UnauthorizedAccessException();

        session.Status = InterviewStatus.InProgress;
        session.StartedAt = DateTime.UtcNow;
        await _sessionRepo.UpdateAsync(session.Id, session, ct);
    }

    public async Task CompleteSessionAsync(string sessionId, string userId, CancellationToken ct = default)
    {
        var session = await _sessionRepo.GetByIdAsync(sessionId, ct)
            ?? throw new InvalidOperationException("Session not found.");

        if (session.UserId != userId) throw new UnauthorizedAccessException();

        session.Status = InterviewStatus.Completed;
        session.CompletedAt = DateTime.UtcNow;

        if (session.StartedAt.HasValue)
            session.DurationMinutes = (session.CompletedAt.Value - session.StartedAt.Value).TotalMinutes;

        await _sessionRepo.UpdateAsync(session.Id, session, ct);
    }

    internal static InterviewSessionDto MapToDto(InterviewSession s) => new(
        s.Id, s.Title, s.JobRole, s.ExperienceLevel, s.Skills,
        s.InterviewType.ToString(), s.Difficulty.ToString(), s.Status.ToString(),
        s.OverallScore, s.TechnicalScore, s.CommunicationScore,
        s.ConfidenceScore, s.ProblemSolvingScore, s.DurationMinutes,
        s.CreatedAt, s.StartedAt, s.CompletedAt,
        s.SkillGapAnalysis is null ? null : new SkillGapAnalysisDto(
            s.SkillGapAnalysis.Strengths,
            s.SkillGapAnalysis.Weaknesses,
            s.SkillGapAnalysis.MissingTopics,
            s.SkillGapAnalysis.Recommendations,
            s.SkillGapAnalysis.SkillScores
        )
    );
}
