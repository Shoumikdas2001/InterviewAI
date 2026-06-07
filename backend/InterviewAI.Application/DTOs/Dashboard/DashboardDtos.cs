namespace InterviewAI.Application.DTOs.Dashboard;

public record DashboardDto(
    int TotalInterviews,
    double AverageScore,
    double HighestScore,
    double ImprovementPercentage,
    double TotalPracticeHours,
    List<ScoreTrendDto> ScoreTrend,
    List<SkillPerformanceDto> SkillPerformance,
    List<RecentInterviewDto> RecentInterviews,
    List<WeakAreaDto> WeakAreas
);

public record ScoreTrendDto(
    string Date,
    double Score,
    string JobRole
);

public record SkillPerformanceDto(
    string Skill,
    double Score,
    int Count
);

public record RecentInterviewDto(
    string Id,
    string Title,
    string JobRole,
    string Status,
    double? OverallScore,
    DateTime CreatedAt
);

public record WeakAreaDto(
    string Topic,
    int Occurrences,
    double AverageScore
);

public record AdminDashboardDto(
    int TotalUsers,
    int ActiveUsers,
    int TotalInterviews,
    int TotalResumes,
    int InterviewsThisMonth,
    int NewUsersThisMonth,
    List<TopSkillDto> TopSkills,
    List<AdminUserDto> RecentUsers
);

public record TopSkillDto(string Skill, int Count);

public record AdminUserDto(
    string Id,
    string FullName,
    string Email,
    bool IsActive,
    int TotalInterviews,
    double AverageScore,
    DateTime CreatedAt
);
