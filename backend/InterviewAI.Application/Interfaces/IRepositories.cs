using InterviewAI.Domain.Entities;
using System.Linq.Expressions;

namespace InterviewAI.Application.Interfaces;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<IEnumerable<User>> GetPagedAsync(int page, int pageSize, CancellationToken ct = default);
    Task UpdateLastLoginAsync(string userId, CancellationToken ct = default);
    Task UpdatePasswordAsync(string userId, string passwordHash, CancellationToken ct = default);
    Task IncrementInterviewCountAsync(string userId, double newScore, CancellationToken ct = default);
}

public interface IResumeRepository : IRepository<Resume>
{
    Task<IEnumerable<Resume>> GetByUserIdAsync(string userId, CancellationToken ct = default);
    Task<Resume?> GetActiveResumeAsync(string userId, CancellationToken ct = default);
    Task UpdateAtsAnalysisAsync(string resumeId, AtsAnalysis analysis, CancellationToken ct = default);
    Task UpdateParsedContentAsync(string resumeId, ParsedResumeContent content, CancellationToken ct = default);
}

public interface IInterviewSessionRepository : IRepository<InterviewSession>
{
    Task<IEnumerable<InterviewSession>> GetByUserIdAsync(string userId, int page, int pageSize, CancellationToken ct = default);
    Task<IEnumerable<InterviewSession>> GetRecentByUserIdAsync(string userId, int count, CancellationToken ct = default);
    Task UpdateStatusAsync(string sessionId, InterviewSession session, CancellationToken ct = default);
    Task<long> CountByUserIdAsync(string userId, CancellationToken ct = default);
    Task<double> GetAverageScoreByUserIdAsync(string userId, CancellationToken ct = default);
}

public interface IQuestionRepository : IRepository<Question>
{
    Task<IEnumerable<Question>> GetBySessionIdAsync(string sessionId, CancellationToken ct = default);
    Task CreateManyAsync(IEnumerable<Question> questions, CancellationToken ct = default);
}

public interface IAnswerRepository : IRepository<Answer>
{
    Task<IEnumerable<Answer>> GetBySessionIdAsync(string sessionId, CancellationToken ct = default);
    Task<Answer?> GetByQuestionIdAsync(string questionId, CancellationToken ct = default);
    Task UpdateEvaluationAsync(string answerId, AnswerEvaluation evaluation, CancellationToken ct = default);
}

public interface IRefreshTokenRepository : IRepository<RefreshToken>
{
    Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken ct = default);
    Task RevokeAllForUserAsync(string userId, CancellationToken ct = default);
    Task MarkAsUsedAsync(string tokenId, CancellationToken ct = default);
}

public interface IStudyPlanRepository : IRepository<StudyPlan>
{
    Task<IEnumerable<StudyPlan>> GetByUserIdAsync(string userId, CancellationToken ct = default);
    Task<StudyPlan?> GetLatestByUserIdAsync(string userId, CancellationToken ct = default);
}

public interface IReportRepository : IRepository<Report>
{
    Task<IEnumerable<Report>> GetByUserIdAsync(string userId, CancellationToken ct = default);
    Task<Report?> GetBySessionIdAsync(string sessionId, CancellationToken ct = default);
}

public interface IAuditLogRepository : IRepository<AuditLog>
{
    Task<IEnumerable<AuditLog>> GetByUserIdAsync(string userId, int page, int pageSize, CancellationToken ct = default);
    Task LogAsync(string? userId, string action, string entity, string? entityId = null,
        string? details = null, string? ipAddress = null, bool isSuccess = true,
        string? errorMessage = null, CancellationToken ct = default);
}
