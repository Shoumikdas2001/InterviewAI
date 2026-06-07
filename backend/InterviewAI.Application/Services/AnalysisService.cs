using InterviewAI.Application.DTOs.Interview;
using InterviewAI.Application.Interfaces;
using InterviewAI.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace InterviewAI.Application.Services;

public class AnalysisService : IAnalysisService
{
    private readonly IInterviewSessionRepository _sessionRepo;
    private readonly IAnswerRepository _answerRepo;
    private readonly IQuestionRepository _questionRepo;
    private readonly IStudyPlanRepository _studyPlanRepo;
    private readonly IAIProvider _aiProvider;
    private readonly ILogger<AnalysisService> _logger;

    public AnalysisService(
        IInterviewSessionRepository sessionRepo,
        IAnswerRepository answerRepo,
        IQuestionRepository questionRepo,
        IStudyPlanRepository studyPlanRepo,
        IAIProvider aiProvider,
        ILogger<AnalysisService> logger)
    {
        _sessionRepo = sessionRepo;
        _answerRepo = answerRepo;
        _questionRepo = questionRepo;
        _studyPlanRepo = studyPlanRepo;
        _aiProvider = aiProvider;
        _logger = logger;
    }

    public async Task<SkillGapAnalysisDto> GenerateSkillGapAnalysisAsync(
        string sessionId, string userId, CancellationToken ct = default)
    {
        var session = await _sessionRepo.GetByIdAsync(sessionId, ct)
            ?? throw new InvalidOperationException("Session not found.");
        if (session.UserId != userId) throw new UnauthorizedAccessException();

        var answers = (await _answerRepo.GetBySessionIdAsync(sessionId, ct)).ToList();
        var questions = (await _questionRepo.GetBySessionIdAsync(sessionId, ct)).ToList();

        if (!answers.Any())
            throw new InvalidOperationException("No answers found. Complete the interview first.");

        var contextParts = questions.Zip(answers, (q, a) =>
            $"Skill: {q.SkillTag}\nQ: {q.QuestionText}\nA: {a.AnswerText}\nScore: {(a.Evaluation?.OverallScore ?? 0)}/10"
        );
        var contextText = string.Join("\n\n", contextParts);
        var skillsList = string.Join(", ", session.Skills);

        var prompt =
            "You are an expert technical career coach analyzing an interview performance.\n\n" +
            $"Job Role: {session.JobRole}\n" +
            $"Experience Level: {session.ExperienceLevel}\n" +
            $"Skills Assessed: {skillsList}\n\n" +
            "INTERVIEW Q&A WITH SCORES:\n" +
            contextText +
            "\n\nProvide a comprehensive skill gap analysis.\n\n" +
            "Return JSON:\n" +
            "{\n" +
            "  \"strengths\": [\"<strength statement 1>\", \"<strength statement 2>\", ...],\n" +
            "  \"weaknesses\": [\"<weakness statement 1>\", \"<weakness statement 2>\", ...],\n" +
            "  \"missingTopics\": [\"<topic 1>\", \"<topic 2>\", ...],\n" +
            "  \"recommendations\": [\"<actionable recommendation 1>\", ...],\n" +
            "  \"skillScores\": {\n" +
            "    \"<skill_name>\": <score_0_to_100>\n" +
            "  }\n" +
            "}\n\n" +
            "Be specific and actionable. Provide at least 3 items in each list.";

        var result = await _aiProvider.GenerateStructuredResponseAsync<GeminiSkillGapResponse>(prompt, ct);
        if (result is null) throw new InvalidOperationException("Skill gap analysis failed.");

        session.SkillGapAnalysis = new SkillGapAnalysis
        {
            Strengths = result.Strengths,
            Weaknesses = result.Weaknesses,
            MissingTopics = result.MissingTopics,
            Recommendations = result.Recommendations,
            SkillScores = result.SkillScores
        };

        var evaluated = answers.Where(a => a.Evaluation is not null).ToList();
        if (evaluated.Any())
        {
            session.OverallScore = Math.Round(evaluated.Average(a => a.Evaluation!.OverallScore), 2);
            session.TechnicalScore = Math.Round(evaluated.Average(a => a.Evaluation!.TechnicalScore), 2);
            session.CommunicationScore = Math.Round(evaluated.Average(a => a.Evaluation!.CommunicationScore), 2);
            session.ConfidenceScore = Math.Round(evaluated.Average(a => a.Evaluation!.ConfidenceScore), 2);
            session.ProblemSolvingScore = Math.Round(evaluated.Average(a => a.Evaluation!.ProblemSolvingScore), 2);
        }

        await _sessionRepo.UpdateAsync(session.Id, session, ct);

        return new SkillGapAnalysisDto(
            result.Strengths, result.Weaknesses,
            result.MissingTopics, result.Recommendations, result.SkillScores
        );
    }

    public async Task<StudyPlanDto> GenerateRoadmapAsync(
        string userId, GenerateRoadmapRequest request, CancellationToken ct = default)
    {
        var weakAreasList = string.Join(", ", request.WeakAreas);

        var prompt =
            "You are a senior software engineering mentor creating a personalized learning roadmap.\n\n" +
            $"Job Role Target: {request.JobRole}\n" +
            $"Weak Areas to Address: {weakAreasList}\n\n" +
            "Create a focused 4-week learning roadmap to address these gaps.\n\n" +
            "Rules:\n" +
            "- Be specific with week themes\n" +
            "- Topics should build on each other progressively\n" +
            "- Include practical resources and exercises\n" +
            "- Prioritize topics by importance to the job role\n" +
            "- Each week should have 3-4 topics\n\n" +
            "Return JSON:\n" +
            "{\n" +
            "  \"title\": \"<roadmap title>\",\n" +
            "  \"estimatedHours\": <total hours integer>,\n" +
            "  \"weeks\": [\n" +
            "    {\n" +
            "      \"weekNumber\": 1,\n" +
            "      \"theme\": \"<week theme>\",\n" +
            "      \"topics\": [\n" +
            "        {\n" +
            "          \"name\": \"<topic name>\",\n" +
            "          \"description\": \"<what to learn>\",\n" +
            "          \"priority\": \"High|Medium|Low\"\n" +
            "        }\n" +
            "      ],\n" +
            "      \"resources\": [\"<resource 1>\", \"<resource 2>\"],\n" +
            "      \"exercises\": [\"<exercise 1>\", \"<exercise 2>\"],\n" +
            "      \"estimatedHours\": <hours integer>\n" +
            "    }\n" +
            "  ]\n" +
            "}";

        var result = await _aiProvider.GenerateStructuredResponseAsync<GeminiRoadmapResponse>(prompt, ct);
        if (result is null) throw new InvalidOperationException("Roadmap generation failed.");

        var studyPlan = new StudyPlan
        {
            UserId = userId,
            SessionId = request.SessionId,
            Title = result.Title,
            JobRole = request.JobRole,
            TargetSkills = request.WeakAreas,
            EstimatedHours = result.EstimatedHours,
            Weeks = result.Weeks.Select(w => new StudyWeek
            {
                WeekNumber = w.WeekNumber,
                Theme = w.Theme,
                Topics = w.Topics.Select(t => new StudyTopic
                {
                    Name = t.Name,
                    Description = t.Description,
                    Priority = t.Priority
                }).ToList(),
                Resources = w.Resources,
                Exercises = w.Exercises,
                EstimatedHours = w.EstimatedHours
            }).ToList()
        };

        await _studyPlanRepo.CreateAsync(studyPlan, ct);

        return MapStudyPlanToDto(studyPlan);
    }

    public async Task<StudyPlanDto?> GetLatestRoadmapAsync(string userId, CancellationToken ct = default)
    {
        var plan = await _studyPlanRepo.GetLatestByUserIdAsync(userId, ct);
        return plan is null ? null : MapStudyPlanToDto(plan);
    }

    private static StudyPlanDto MapStudyPlanToDto(StudyPlan p) => new(
        p.Id, p.Title, p.JobRole, p.TargetSkills, p.EstimatedHours,
        p.Weeks.Select(w => new StudyWeekDto(
            w.WeekNumber, w.Theme,
            w.Topics.Select(t => new StudyTopicDto(t.Name, t.Description, t.Priority)).ToList(),
            w.Resources, w.Exercises, w.EstimatedHours
        )).ToList(),
        p.CreatedAt
    );
}
