using MongoDB.Driver;
using InterviewAI.Application.Interfaces;
using InterviewAI.Domain.Entities;
using InterviewAI.Infrastructure.Mongo;

namespace InterviewAI.Infrastructure.Repositories;

public class UserRepository : MongoRepository<User>, IUserRepository
{
    public UserRepository(MongoDbContext context) : base(context.Users) { }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
    {
        return await _collection.Find(u => u.Email == email.ToLowerInvariant()).FirstOrDefaultAsync(ct);
    }

    public async Task<IEnumerable<User>> GetPagedAsync(int page, int pageSize, CancellationToken ct = default)
    {
        return await _collection.Find(_ => true)
            .SortByDescending(u => u.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync(ct);
    }

    public async Task UpdateLastLoginAsync(string userId, CancellationToken ct = default)
    {
        var filter = Builders<User>.Filter.Eq(u => u.Id, userId);
        var update = Builders<User>.Update
            .Set(u => u.LastLoginAt, DateTime.UtcNow)
            .Set(u => u.UpdatedAt, DateTime.UtcNow);
        await _collection.UpdateOneAsync(filter, update, cancellationToken: ct);
    }

    public async Task UpdatePasswordAsync(string userId, string passwordHash, CancellationToken ct = default)
    {
        var filter = Builders<User>.Filter.Eq(u => u.Id, userId);
        var update = Builders<User>.Update
            .Set(u => u.PasswordHash, passwordHash)
            .Set(u => u.PasswordResetToken, null)
            .Set(u => u.PasswordResetExpiry, null)
            .Set(u => u.UpdatedAt, DateTime.UtcNow);
        await _collection.UpdateOneAsync(filter, update, cancellationToken: ct);
    }

    public async Task IncrementInterviewCountAsync(string userId, double newScore, CancellationToken ct = default)
    {
        // Fetch current to compute running average
        var user = await GetByIdAsync(userId, ct);
        if (user is null) return;

        var newTotal = user.TotalInterviews + 1;
        var newAvg = ((user.AverageScore * user.TotalInterviews) + newScore) / newTotal;

        var filter = Builders<User>.Filter.Eq(u => u.Id, userId);
        var update = Builders<User>.Update
            .Set(u => u.TotalInterviews, newTotal)
            .Set(u => u.AverageScore, Math.Round(newAvg, 2))
            .Set(u => u.UpdatedAt, DateTime.UtcNow);
        await _collection.UpdateOneAsync(filter, update, cancellationToken: ct);
    }
}

public class ResumeRepository : MongoRepository<Resume>, IResumeRepository
{
    public ResumeRepository(MongoDbContext context) : base(context.Resumes) { }

    public async Task<IEnumerable<Resume>> GetByUserIdAsync(string userId, CancellationToken ct = default)
    {
        return await _collection.Find(r => r.UserId == userId)
            .SortByDescending(r => r.UploadedAt)
            .ToListAsync(ct);
    }

    public async Task<Resume?> GetActiveResumeAsync(string userId, CancellationToken ct = default)
    {
        return await _collection.Find(r => r.UserId == userId && r.IsActive)
            .SortByDescending(r => r.UploadedAt)
            .FirstOrDefaultAsync(ct);
    }

    public async Task UpdateAtsAnalysisAsync(string resumeId, AtsAnalysis analysis, CancellationToken ct = default)
    {
        var filter = Builders<Resume>.Filter.Eq(r => r.Id, resumeId);
        var update = Builders<Resume>.Update
            .Set(r => r.AtsAnalysis, analysis)
            .Set(r => r.AnalyzedAt, DateTime.UtcNow);
        await _collection.UpdateOneAsync(filter, update, cancellationToken: ct);
    }

    public async Task UpdateParsedContentAsync(string resumeId, ParsedResumeContent content, CancellationToken ct = default)
    {
        var filter = Builders<Resume>.Filter.Eq(r => r.Id, resumeId);
        var update = Builders<Resume>.Update.Set(r => r.ParsedContent, content);
        await _collection.UpdateOneAsync(filter, update, cancellationToken: ct);
    }
}

public class InterviewSessionRepository : MongoRepository<InterviewSession>, IInterviewSessionRepository
{
    public InterviewSessionRepository(MongoDbContext context) : base(context.InterviewSessions) { }

    public async Task<IEnumerable<InterviewSession>> GetByUserIdAsync(string userId, int page, int pageSize, CancellationToken ct = default)
    {
        return await _collection.Find(s => s.UserId == userId)
            .SortByDescending(s => s.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<InterviewSession>> GetRecentByUserIdAsync(string userId, int count, CancellationToken ct = default)
    {
        return await _collection.Find(s => s.UserId == userId)
            .SortByDescending(s => s.CreatedAt)
            .Limit(count)
            .ToListAsync(ct);
    }

    public async Task UpdateStatusAsync(string sessionId, InterviewSession session, CancellationToken ct = default)
    {
        await UpdateAsync(sessionId, session, ct);
    }

    public async Task<long> CountByUserIdAsync(string userId, CancellationToken ct = default)
    {
        return await _collection.CountDocumentsAsync(s => s.UserId == userId, cancellationToken: ct);
    }

    public async Task<double> GetAverageScoreByUserIdAsync(string userId, CancellationToken ct = default)
    {
        var sessions = await _collection.Find(s => s.UserId == userId && s.OverallScore.HasValue).ToListAsync(ct);
        if (!sessions.Any()) return 0;
        return sessions.Average(s => s.OverallScore!.Value);
    }
}

public class QuestionRepository : MongoRepository<Question>, IQuestionRepository
{
    public QuestionRepository(MongoDbContext context) : base(context.Questions) { }

    public async Task<IEnumerable<Question>> GetBySessionIdAsync(string sessionId, CancellationToken ct = default)
    {
        return await _collection.Find(q => q.SessionId == sessionId)
            .SortBy(q => q.OrderIndex)
            .ToListAsync(ct);
    }

    public async Task CreateManyAsync(IEnumerable<Question> questions, CancellationToken ct = default)
    {
        await _collection.InsertManyAsync(questions, cancellationToken: ct);
    }
}

public class AnswerRepository : MongoRepository<Answer>, IAnswerRepository
{
    public AnswerRepository(MongoDbContext context) : base(context.Answers) { }

    public async Task<IEnumerable<Answer>> GetBySessionIdAsync(string sessionId, CancellationToken ct = default)
    {
        return await _collection.Find(a => a.SessionId == sessionId)
            .SortBy(a => a.SubmittedAt)
            .ToListAsync(ct);
    }

    public async Task<Answer?> GetByQuestionIdAsync(string questionId, CancellationToken ct = default)
    {
        return await _collection.Find(a => a.QuestionId == questionId).FirstOrDefaultAsync(ct);
    }

    public async Task UpdateEvaluationAsync(string answerId, AnswerEvaluation evaluation, CancellationToken ct = default)
    {
        var filter = Builders<Answer>.Filter.Eq(a => a.Id, answerId);
        var update = Builders<Answer>.Update.Set(a => a.Evaluation, evaluation);
        await _collection.UpdateOneAsync(filter, update, cancellationToken: ct);
    }
}

public class RefreshTokenRepository : MongoRepository<RefreshToken>, IRefreshTokenRepository
{
    public RefreshTokenRepository(MongoDbContext context) : base(context.RefreshTokens) { }

    public async Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken ct = default)
    {
        return await _collection.Find(t => t.Token == token).FirstOrDefaultAsync(ct);
    }

    public async Task RevokeAllForUserAsync(string userId, CancellationToken ct = default)
    {
        var filter = Builders<RefreshToken>.Filter.Eq(t => t.UserId, userId);
        var update = Builders<RefreshToken>.Update.Set(t => t.IsRevoked, true);
        await _collection.UpdateManyAsync(filter, update, cancellationToken: ct);
    }

    public async Task MarkAsUsedAsync(string tokenId, CancellationToken ct = default)
    {
        var filter = Builders<RefreshToken>.Filter.Eq(t => t.Id, tokenId);
        var update = Builders<RefreshToken>.Update.Set(t => t.IsUsed, true);
        await _collection.UpdateOneAsync(filter, update, cancellationToken: ct);
    }
}

public class StudyPlanRepository : MongoRepository<StudyPlan>, IStudyPlanRepository
{
    public StudyPlanRepository(MongoDbContext context) : base(context.StudyPlans) { }

    public async Task<IEnumerable<StudyPlan>> GetByUserIdAsync(string userId, CancellationToken ct = default)
    {
        return await _collection.Find(sp => sp.UserId == userId)
            .SortByDescending(sp => sp.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<StudyPlan?> GetLatestByUserIdAsync(string userId, CancellationToken ct = default)
    {
        return await _collection.Find(sp => sp.UserId == userId)
            .SortByDescending(sp => sp.CreatedAt)
            .FirstOrDefaultAsync(ct);
    }
}

public class ReportRepository : MongoRepository<Report>, IReportRepository
{
    public ReportRepository(MongoDbContext context) : base(context.Reports) { }

    public async Task<IEnumerable<Report>> GetByUserIdAsync(string userId, CancellationToken ct = default)
    {
        return await _collection.Find(r => r.UserId == userId)
            .SortByDescending(r => r.GeneratedAt)
            .ToListAsync(ct);
    }

    public async Task<Report?> GetBySessionIdAsync(string sessionId, CancellationToken ct = default)
    {
        return await _collection.Find(r => r.SessionId == sessionId).FirstOrDefaultAsync(ct);
    }
}

public class AuditLogRepository : MongoRepository<AuditLog>, IAuditLogRepository
{
    public AuditLogRepository(MongoDbContext context) : base(context.AuditLogs) { }

    public async Task<IEnumerable<AuditLog>> GetByUserIdAsync(string userId, int page, int pageSize, CancellationToken ct = default)
    {
        return await _collection.Find(a => a.UserId == userId)
            .SortByDescending(a => a.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync(ct);
    }

    public async Task LogAsync(string? userId, string action, string entity, string? entityId = null,
        string? details = null, string? ipAddress = null, bool isSuccess = true,
        string? errorMessage = null, CancellationToken ct = default)
    {
        var log = new AuditLog
        {
            UserId = userId,
            Action = action,
            Entity = entity,
            EntityId = entityId,
            Details = details,
            IpAddress = ipAddress,
            IsSuccess = isSuccess,
            ErrorMessage = errorMessage
        };
        await CreateAsync(log, ct);
    }
}
