using MongoDB.Driver;
using InterviewAI.Domain.Entities;
using Microsoft.Extensions.Configuration;

namespace InterviewAI.Infrastructure.Mongo;

public class MongoDbContext
{
    private readonly IMongoDatabase _database;

    public MongoDbContext(IConfiguration configuration)
    {
        var connectionString = configuration["MongoDB:ConnectionString"]
            ?? throw new InvalidOperationException("MongoDB connection string is not configured.");
        var databaseName = configuration["MongoDB:DatabaseName"]
            ?? throw new InvalidOperationException("MongoDB database name is not configured.");

        var client = new MongoClient(connectionString);
        _database = client.GetDatabase(databaseName);

        ConfigureConventions();
        EnsureIndexes();
    }

    private static void ConfigureConventions()
    {
        var pack = new MongoDB.Bson.Serialization.Conventions.ConventionPack
        {
            new MongoDB.Bson.Serialization.Conventions.CamelCaseElementNameConvention(),
            new MongoDB.Bson.Serialization.Conventions.IgnoreExtraElementsConvention(true)
        };
        MongoDB.Bson.Serialization.Conventions.ConventionRegistry.Register("InterviewAI", pack, _ => true);
    }

    public IMongoCollection<User> Users => _database.GetCollection<User>("users");
    public IMongoCollection<Resume> Resumes => _database.GetCollection<Resume>("resumes");
    public IMongoCollection<InterviewSession> InterviewSessions => _database.GetCollection<InterviewSession>("interviewSessions");
    public IMongoCollection<Question> Questions => _database.GetCollection<Question>("questions");
    public IMongoCollection<Answer> Answers => _database.GetCollection<Answer>("answers");
    public IMongoCollection<StudyPlan> StudyPlans => _database.GetCollection<StudyPlan>("studyPlans");
    public IMongoCollection<Report> Reports => _database.GetCollection<Report>("reports");
    public IMongoCollection<RefreshToken> RefreshTokens => _database.GetCollection<RefreshToken>("refreshTokens");
    public IMongoCollection<AuditLog> AuditLogs => _database.GetCollection<AuditLog>("auditLogs");

    private void EnsureIndexes()
    {
        // Users
        Users.Indexes.CreateMany(
        [
            new CreateIndexModel<User>(Builders<User>.IndexKeys.Ascending(u => u.Email),
                new CreateIndexOptions { Unique = true, Name = "idx_users_email" }),
            new CreateIndexModel<User>(Builders<User>.IndexKeys.Ascending(u => u.Role),
                new CreateIndexOptions { Name = "idx_users_role" })
        ]);

        // Resumes
        Resumes.Indexes.CreateMany(
        [
            new CreateIndexModel<Resume>(Builders<Resume>.IndexKeys.Ascending(r => r.UserId),
                new CreateIndexOptions { Name = "idx_resumes_userId" }),
            new CreateIndexModel<Resume>(Builders<Resume>.IndexKeys.Descending(r => r.UploadedAt),
                new CreateIndexOptions { Name = "idx_resumes_uploadedAt" })
        ]);

        // InterviewSessions
        InterviewSessions.Indexes.CreateMany(
        [
            new CreateIndexModel<InterviewSession>(Builders<InterviewSession>.IndexKeys.Ascending(s => s.UserId),
                new CreateIndexOptions { Name = "idx_sessions_userId" }),
            new CreateIndexModel<InterviewSession>(Builders<InterviewSession>.IndexKeys.Descending(s => s.CreatedAt),
                new CreateIndexOptions { Name = "idx_sessions_createdAt" }),
            new CreateIndexModel<InterviewSession>(Builders<InterviewSession>.IndexKeys.Ascending(s => s.Status),
                new CreateIndexOptions { Name = "idx_sessions_status" })
        ]);

        // Questions
        Questions.Indexes.CreateMany(
        [
            new CreateIndexModel<Question>(Builders<Question>.IndexKeys.Ascending(q => q.SessionId),
                new CreateIndexOptions { Name = "idx_questions_sessionId" }),
            new CreateIndexModel<Question>(Builders<Question>.IndexKeys.Ascending(q => q.UserId),
                new CreateIndexOptions { Name = "idx_questions_userId" })
        ]);

        // Answers
        Answers.Indexes.CreateMany(
        [
            new CreateIndexModel<Answer>(Builders<Answer>.IndexKeys.Ascending(a => a.SessionId),
                new CreateIndexOptions { Name = "idx_answers_sessionId" }),
            new CreateIndexModel<Answer>(Builders<Answer>.IndexKeys.Ascending(a => a.QuestionId),
                new CreateIndexOptions { Name = "idx_answers_questionId" })
        ]);

        // RefreshTokens
        RefreshTokens.Indexes.CreateMany(
        [
            new CreateIndexModel<RefreshToken>(Builders<RefreshToken>.IndexKeys.Ascending(t => t.Token),
                new CreateIndexOptions { Unique = true, Name = "idx_refreshTokens_token" }),
            new CreateIndexModel<RefreshToken>(Builders<RefreshToken>.IndexKeys.Ascending(t => t.UserId),
                new CreateIndexOptions { Name = "idx_refreshTokens_userId" }),
            new CreateIndexModel<RefreshToken>(Builders<RefreshToken>.IndexKeys.Ascending(t => t.ExpiresAt),
                new CreateIndexOptions
                {
                    Name = "idx_refreshTokens_ttl",
                    ExpireAfter = TimeSpan.FromDays(30) // TTL index — auto-cleanup expired tokens
                })
        ]);

        // AuditLogs
        AuditLogs.Indexes.CreateMany(
        [
            new CreateIndexModel<AuditLog>(Builders<AuditLog>.IndexKeys.Ascending(a => a.UserId),
                new CreateIndexOptions { Name = "idx_auditLogs_userId" }),
            new CreateIndexModel<AuditLog>(Builders<AuditLog>.IndexKeys.Descending(a => a.CreatedAt),
                new CreateIndexOptions { Name = "idx_auditLogs_createdAt" })
        ]);

        // StudyPlans
        StudyPlans.Indexes.CreateOne(
            new CreateIndexModel<StudyPlan>(Builders<StudyPlan>.IndexKeys.Ascending(sp => sp.UserId),
                new CreateIndexOptions { Name = "idx_studyPlans_userId" }));
    }
}
