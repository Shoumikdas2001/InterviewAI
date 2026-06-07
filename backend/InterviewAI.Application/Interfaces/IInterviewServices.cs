using InterviewAI.Application.DTOs.Interview;

namespace InterviewAI.Application.Interfaces;

public interface IInterviewService
{
    Task<InterviewWithQuestionsDto> CreateInterviewAsync(string userId, CreateInterviewRequest request, CancellationToken ct = default);
    Task<InterviewSessionDto?> GetSessionAsync(string sessionId, string userId, CancellationToken ct = default);
    Task<IEnumerable<InterviewSessionDto>> GetHistoryAsync(string userId, int page, int pageSize, CancellationToken ct = default);
    Task StartSessionAsync(string sessionId, string userId, CancellationToken ct = default);
    Task CompleteSessionAsync(string sessionId, string userId, CancellationToken ct = default);
}

public interface IQuestionService
{
    Task<List<QuestionDto>> GenerateQuestionsAsync(string sessionId, string userId, CancellationToken ct = default);
    Task<List<QuestionDto>> GenerateFollowUpQuestionsAsync(GenerateFollowUpRequest request, string userId, CancellationToken ct = default);
    Task<IEnumerable<QuestionDto>> GetSessionQuestionsAsync(string sessionId, string userId, CancellationToken ct = default);
}

public interface IAnswerService
{
    Task<AnswerDto> SubmitAnswerAsync(string userId, SubmitAnswerRequest request, CancellationToken ct = default);
    Task<AnswerEvaluationDto> EvaluateAnswerAsync(string answerId, string userId, CancellationToken ct = default);
    Task<IEnumerable<AnswerDto>> GetSessionAnswersAsync(string sessionId, string userId, CancellationToken ct = default);
}

public interface IAnalysisService
{
    Task<SkillGapAnalysisDto> GenerateSkillGapAnalysisAsync(string sessionId, string userId, CancellationToken ct = default);
    Task<StudyPlanDto> GenerateRoadmapAsync(string userId, GenerateRoadmapRequest request, CancellationToken ct = default);
    Task<StudyPlanDto?> GetLatestRoadmapAsync(string userId, CancellationToken ct = default);
}

public interface IDashboardService
{
    Task<DTOs.Dashboard.DashboardDto> GetDashboardAsync(string userId, CancellationToken ct = default);
    Task<DTOs.Dashboard.AdminDashboardDto> GetAdminDashboardAsync(CancellationToken ct = default);
}

public interface IReportService
{
    Task<byte[]> GenerateReportAsync(string sessionId, string userId, CancellationToken ct = default);
    Task<byte[]> GetReportAsync(string reportId, string userId, CancellationToken ct = default);
}
