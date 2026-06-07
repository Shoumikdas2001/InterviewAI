using InterviewAI.Application.DTOs.Dashboard;
using InterviewAI.Application.Interfaces;
using InterviewAI.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace InterviewAI.Application.Services;

public class DashboardService : IDashboardService
{
    private readonly IInterviewSessionRepository _sessionRepo;
    private readonly IUserRepository _userRepo;
    private readonly IResumeRepository _resumeRepo;
    private readonly IAnswerRepository _answerRepo;
    private readonly ILogger<DashboardService> _logger;

    public DashboardService(
        IInterviewSessionRepository sessionRepo,
        IUserRepository userRepo,
        IResumeRepository resumeRepo,
        IAnswerRepository answerRepo,
        ILogger<DashboardService> logger)
    {
        _sessionRepo = sessionRepo;
        _userRepo = userRepo;
        _resumeRepo = resumeRepo;
        _answerRepo = answerRepo;
        _logger = logger;
    }

    public async Task<DashboardDto> GetDashboardAsync(string userId, CancellationToken ct = default)
    {
        var sessions = (await _sessionRepo.GetByUserIdAsync(userId, 1, 100, ct)).ToList();
        var completedSessions = sessions.Where(s => s.Status == InterviewStatus.Completed).ToList();

        var totalInterviews = sessions.Count;
        var averageScore = completedSessions.Any()
            ? Math.Round(completedSessions.Average(s => s.OverallScore ?? 0), 2)
            : 0;
        var highestScore = completedSessions.Any()
            ? completedSessions.Max(s => s.OverallScore ?? 0)
            : 0;

        // Improvement: compare first half avg vs second half avg
        double improvementPercentage = 0;
        if (completedSessions.Count >= 4)
        {
            var half = completedSessions.Count / 2;
            var firstHalfAvg = completedSessions.Take(half).Average(s => s.OverallScore ?? 0);
            var secondHalfAvg = completedSessions.Skip(half).Average(s => s.OverallScore ?? 0);
            if (firstHalfAvg > 0)
                improvementPercentage = Math.Round(((secondHalfAvg - firstHalfAvg) / firstHalfAvg) * 100, 1);
        }

        var totalPracticeHours = completedSessions.Sum(s => s.DurationMinutes ?? 30) / 60.0;

        // Score trend — last 10 sessions
        var scoreTrend = completedSessions
            .OrderBy(s => s.CreatedAt)
            .TakeLast(10)
            .Select(s => new ScoreTrendDto(
                s.CreatedAt.ToString("MMM d"),
                Math.Round(s.OverallScore ?? 0, 1),
                s.JobRole
            )).ToList();

        // Skill performance from skill gap analyses
        var skillPerformance = completedSessions
            .Where(s => s.SkillGapAnalysis?.SkillScores.Any() == true)
            .SelectMany(s => s.SkillGapAnalysis!.SkillScores)
            .GroupBy(kv => kv.Key)
            .Select(g => new SkillPerformanceDto(g.Key, Math.Round(g.Average(kv => kv.Value), 1), g.Count()))
            .OrderByDescending(s => s.Count)
            .Take(8)
            .ToList();

        // Weak areas — most mentioned missing topics
        var weakAreas = completedSessions
            .Where(s => s.SkillGapAnalysis?.MissingTopics.Any() == true)
            .SelectMany(s => s.SkillGapAnalysis!.MissingTopics)
            .GroupBy(t => t)
            .Select(g => new WeakAreaDto(g.Key, g.Count(), 0))
            .OrderByDescending(w => w.Occurrences)
            .Take(5)
            .ToList();

        // Recent interviews
        var recentInterviews = sessions
            .Take(5)
            .Select(s => new RecentInterviewDto(
                s.Id, s.Title, s.JobRole, s.Status.ToString(),
                s.OverallScore, s.CreatedAt
            )).ToList();

        return new DashboardDto(
            totalInterviews,
            averageScore,
            highestScore,
            improvementPercentage,
            Math.Round(totalPracticeHours, 1),
            scoreTrend,
            skillPerformance,
            recentInterviews,
            weakAreas
        );
    }

    public async Task<AdminDashboardDto> GetAdminDashboardAsync(CancellationToken ct = default)
    {
        var allUsers = (await _userRepo.GetPagedAsync(1, 1000, ct)).ToList();
        var allSessions = (await _sessionRepo.FindAsync(_ => true, ct)).ToList();
        var allResumes = (await _resumeRepo.FindAsync(_ => true, ct)).ToList();

        var now = DateTime.UtcNow;
        var monthStart = new DateTime(now.Year, now.Month, 1);

        var totalUsers = allUsers.Count;
        var activeUsers = allUsers.Count(u => u.IsActive);
        var newUsersThisMonth = allUsers.Count(u => u.CreatedAt >= monthStart);
        var interviewsThisMonth = allSessions.Count(s => s.CreatedAt >= monthStart);

        // Top skills across all sessions
        var topSkills = allSessions
            .SelectMany(s => s.Skills)
            .GroupBy(s => s)
            .OrderByDescending(g => g.Count())
            .Take(10)
            .Select(g => new TopSkillDto(g.Key, g.Count()))
            .ToList();

        // Recent users
        var recentUsers = allUsers
            .OrderByDescending(u => u.CreatedAt)
            .Take(10)
            .Select(u => new AdminUserDto(
                u.Id,
                $"{u.FirstName} {u.LastName}",
                u.Email,
                u.IsActive,
                u.TotalInterviews,
                u.AverageScore,
                u.CreatedAt
            )).ToList();

        return new AdminDashboardDto(
            totalUsers, activeUsers, allSessions.Count, allResumes.Count,
            interviewsThisMonth, newUsersThisMonth, topSkills, recentUsers
        );
    }
}
