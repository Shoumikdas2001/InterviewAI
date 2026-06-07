using InterviewAI.Application.DTOs.Interview;
using InterviewAI.Application.Interfaces;
using InterviewAI.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace InterviewAI.Application.Services;

public class QuestionService : IQuestionService
{
    private readonly IQuestionRepository _questionRepo;
    private readonly IInterviewSessionRepository _sessionRepo;
    private readonly IResumeRepository _resumeRepo;
    private readonly IAnswerRepository _answerRepo;
    private readonly IAIProvider _aiProvider;
    private readonly ILogger<QuestionService> _logger;

    public QuestionService(
        IQuestionRepository questionRepo,
        IInterviewSessionRepository sessionRepo,
        IResumeRepository resumeRepo,
        IAnswerRepository answerRepo,
        IAIProvider aiProvider,
        ILogger<QuestionService> logger)
    {
        _questionRepo = questionRepo;
        _sessionRepo = sessionRepo;
        _resumeRepo = resumeRepo;
        _answerRepo = answerRepo;
        _aiProvider = aiProvider;
        _logger = logger;
    }

    public async Task<List<QuestionDto>> GenerateQuestionsAsync(string sessionId, string userId, CancellationToken ct = default)
    {
        var session = await _sessionRepo.GetByIdAsync(sessionId, ct)
            ?? throw new InvalidOperationException("Session not found.");

        string resumeContext = string.Empty;
        if (!string.IsNullOrEmpty(session.ResumeId))
        {
            var resume = await _resumeRepo.GetByIdAsync(session.ResumeId, ct);
            if (resume?.ParsedContent is not null)
            {
                var skills = string.Join(", ", resume.ParsedContent.Skills.Take(10));
                resumeContext =
                    "\n\nCANDIDATE RESUME CONTEXT:" +
                    $"\nSkills: {skills}" +
                    $"\nYears of Experience: {resume.ParsedContent.TotalYearsExperience}" +
                    $"\nSummary: {resume.ParsedContent.Summary}" +
                    "\n\nGenerate questions that are personalized to this candidate's background.";
            }
        }

        var skillsList = string.Join(", ", session.Skills);
        var prompt =
            "You are a senior technical interviewer at a top tech company.\n\n" +
            "Generate exactly 8 interview questions for the following interview:\n\n" +
            $"Job Role: {session.JobRole}\n" +
            $"Experience Level: {session.ExperienceLevel}\n" +
            $"Skills to Cover: {skillsList}\n" +
            $"Interview Type: {session.InterviewType}\n" +
            $"Difficulty: {session.Difficulty}" +
            resumeContext +
            "\n\nRules:\n" +
            "- Questions must be specific to the skills listed\n" +
            "- Cover different skills, not the same skill twice\n" +
            "- For Technical interviews: focus on technical concepts, code, algorithms\n" +
            "- For Behavioral interviews: use STAR method prompts\n" +
            "- For Mixed: blend technical and behavioral questions\n" +
            "- Questions should match the difficulty level\n\n" +
            "Return JSON with this exact structure:\n" +
            "{\n" +
            "  \"questions\": [\n" +
            "    {\n" +
            "      \"questionText\": \"<the full question>\",\n" +
            "      \"category\": \"<Technical|Behavioral|System Design|Problem Solving>\",\n" +
            "      \"skillTag\": \"<specific skill this tests>\",\n" +
            "      \"expectedKeyPoints\": [\"<key point 1>\", \"<key point 2>\", \"<key point 3>\"]\n" +
            "    }\n" +
            "  ]\n" +
            "}";

        var response = await _aiProvider.GenerateStructuredResponseAsync<GeminiQuestionsResponse>(prompt, ct);
        if (response?.Questions is null || response.Questions.Count == 0)
            throw new InvalidOperationException("AI failed to generate questions.");

        var questions = response.Questions.Select((q, i) => new Question
        {
            SessionId = sessionId,
            UserId = userId,
            QuestionText = q.QuestionText,
            Category = q.Category,
            SkillTag = q.SkillTag,
            OrderIndex = i + 1,
            IsFollowUp = false,
            ExpectedKeyPoints = q.ExpectedKeyPoints
        }).ToList();

        await _questionRepo.CreateManyAsync(questions, ct);

        _logger.LogInformation("Generated {Count} questions for session {SessionId}", questions.Count, sessionId);

        return questions.Select(MapToDto).ToList();
    }

    public async Task<List<QuestionDto>> GenerateFollowUpQuestionsAsync(
        GenerateFollowUpRequest request, string userId, CancellationToken ct = default)
    {
        var session = await _sessionRepo.GetByIdAsync(request.SessionId, ct)
            ?? throw new InvalidOperationException("Session not found.");

        if (session.UserId != userId) throw new UnauthorizedAccessException();

        var question = await _questionRepo.GetByIdAsync(request.QuestionId, ct)
            ?? throw new InvalidOperationException("Question not found.");

        var answer = await _answerRepo.GetByIdAsync(request.AnswerId, ct)
            ?? throw new InvalidOperationException("Answer not found.");

        var prompt =
            "You are a senior technical interviewer conducting a live interview.\n\n" +
            $"The candidate was asked this question:\n\"{question.QuestionText}\"\n\n" +
            $"The candidate answered:\n\"{answer.AnswerText}\"\n\n" +
            "Based on this answer, generate 2 intelligent follow-up questions that:\n" +
            "1. Probe deeper into concepts they mentioned\n" +
            "2. Test their understanding of gaps or surface-level answers\n" +
            "3. Challenge them with edge cases or real-world scenarios\n\n" +
            $"Context:\nJob Role: {session.JobRole}\nSkill Being Tested: {question.SkillTag}\n\n" +
            "Return JSON:\n" +
            "{\n" +
            "  \"questions\": [\n" +
            "    {\n" +
            "      \"questionText\": \"<follow-up question>\",\n" +
            $"      \"category\": \"{question.Category}\",\n" +
            $"      \"skillTag\": \"{question.SkillTag}\",\n" +
            "      \"expectedKeyPoints\": [\"<key point 1>\", \"<key point 2>\"]\n" +
            "    }\n" +
            "  ]\n" +
            "}";

        var response = await _aiProvider.GenerateStructuredResponseAsync<GeminiQuestionsResponse>(prompt, ct);
        if (response?.Questions is null) throw new InvalidOperationException("Follow-up generation failed.");

        var currentCount = (await _questionRepo.GetBySessionIdAsync(request.SessionId, ct)).Count();

        var followUps = response.Questions.Select((q, i) => new Question
        {
            SessionId = request.SessionId,
            UserId = userId,
            QuestionText = q.QuestionText,
            Category = q.Category,
            SkillTag = q.SkillTag,
            OrderIndex = currentCount + i + 1,
            IsFollowUp = true,
            ParentQuestionId = request.QuestionId,
            ExpectedKeyPoints = q.ExpectedKeyPoints
        }).ToList();

        await _questionRepo.CreateManyAsync(followUps, ct);

        return followUps.Select(MapToDto).ToList();
    }

    public async Task<IEnumerable<QuestionDto>> GetSessionQuestionsAsync(string sessionId, string userId, CancellationToken ct = default)
    {
        var session = await _sessionRepo.GetByIdAsync(sessionId, ct);
        if (session is null || session.UserId != userId) return [];

        var questions = await _questionRepo.GetBySessionIdAsync(sessionId, ct);
        return questions.Select(MapToDto);
    }

    private static QuestionDto MapToDto(Question q) => new(
        q.Id, q.SessionId, q.QuestionText, q.Category, q.SkillTag,
        q.OrderIndex, q.IsFollowUp, q.ParentQuestionId, q.ExpectedKeyPoints
    );
}
