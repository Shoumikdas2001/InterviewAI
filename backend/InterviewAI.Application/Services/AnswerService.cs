using InterviewAI.Application.DTOs.Interview;
using InterviewAI.Application.Interfaces;
using InterviewAI.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace InterviewAI.Application.Services;

public class AnswerService : IAnswerService
{
    private readonly IAnswerRepository _answerRepo;
    private readonly IQuestionRepository _questionRepo;
    private readonly IInterviewSessionRepository _sessionRepo;
    private readonly IAIProvider _aiProvider;
    private readonly IUserRepository _userRepo;
    private readonly ILogger<AnswerService> _logger;

    public AnswerService(
        IAnswerRepository answerRepo,
        IQuestionRepository questionRepo,
        IInterviewSessionRepository sessionRepo,
        IAIProvider aiProvider,
        IUserRepository userRepo,
        ILogger<AnswerService> logger)
    {
        _answerRepo = answerRepo;
        _questionRepo = questionRepo;
        _sessionRepo = sessionRepo;
        _aiProvider = aiProvider;
        _userRepo = userRepo;
        _logger = logger;
    }

    public async Task<AnswerDto> SubmitAnswerAsync(string userId, SubmitAnswerRequest request, CancellationToken ct = default)
    {
        var question = await _questionRepo.GetByIdAsync(request.QuestionId, ct)
            ?? throw new InvalidOperationException("Question not found.");

        var answer = new Answer
        {
            QuestionId = request.QuestionId,
            SessionId = request.SessionId,
            UserId = userId,
            AnswerText = request.AnswerText,
            Transcript = request.Transcript,
            IsVoiceAnswer = request.IsVoiceAnswer,
            DurationSeconds = request.DurationSeconds
        };

        await _answerRepo.CreateAsync(answer, ct);
        await EvaluateAndSaveAsync(answer, question, ct);

        var updated = await _answerRepo.GetByIdAsync(answer.Id, ct);
        return MapToDto(updated!);
    }

    public async Task<AnswerEvaluationDto> EvaluateAnswerAsync(string answerId, string userId, CancellationToken ct = default)
    {
        var answer = await _answerRepo.GetByIdAsync(answerId, ct)
            ?? throw new InvalidOperationException("Answer not found.");

        if (answer.UserId != userId) throw new UnauthorizedAccessException();

        var question = await _questionRepo.GetByIdAsync(answer.QuestionId, ct)
            ?? throw new InvalidOperationException("Question not found.");

        await EvaluateAndSaveAsync(answer, question, ct);

        var updated = await _answerRepo.GetByIdAsync(answerId, ct);
        return MapEvaluationToDto(updated!.Evaluation!);
    }

    public async Task<IEnumerable<AnswerDto>> GetSessionAnswersAsync(string sessionId, string userId, CancellationToken ct = default)
    {
        var session = await _sessionRepo.GetByIdAsync(sessionId, ct);
        if (session is null || session.UserId != userId) return [];

        var answers = await _answerRepo.GetBySessionIdAsync(sessionId, ct);
        return answers.Select(MapToDto);
    }

    private async Task EvaluateAndSaveAsync(Answer answer, Question question, CancellationToken ct)
    {
        var keyPoints = string.Join("\n- ", question.ExpectedKeyPoints);

        var prompt =
            "You are an expert technical interviewer evaluating a candidate's answer.\n\n" +
            $"QUESTION: {question.QuestionText}\n" +
            $"SKILL BEING TESTED: {question.SkillTag}\n" +
            $"CATEGORY: {question.Category}\n\n" +
            "EXPECTED KEY POINTS:\n" +
            $"- {keyPoints}\n\n" +
            $"CANDIDATE'S ANSWER: {answer.AnswerText}\n\n" +
            "Evaluate the answer on these dimensions (score 1-10 each):\n" +
            "- Technical Accuracy: Is the answer technically correct?\n" +
            "- Communication: Is it articulate and well-structured?\n" +
            "- Confidence: Does the answer project confidence?\n" +
            "- Clarity: Is it easy to understand?\n" +
            "- Problem Solving: Does it show analytical thinking?\n" +
            "- Completeness: Does it cover the expected key points?\n\n" +
            "Calculate overallScore as weighted average:\n" +
            "(technical*0.35 + communication*0.15 + confidence*0.1 + clarity*0.15 + problemSolving*0.15 + completeness*0.1)\n" +
            "Round to nearest integer.\n\n" +
            "Return JSON with this exact structure:\n" +
            "{\n" +
            "  \"overallScore\": <integer 1-10>,\n" +
            "  \"technicalScore\": <integer 1-10>,\n" +
            "  \"communicationScore\": <integer 1-10>,\n" +
            "  \"confidenceScore\": <integer 1-10>,\n" +
            "  \"clarityScore\": <integer 1-10>,\n" +
            "  \"problemSolvingScore\": <integer 1-10>,\n" +
            "  \"completenessScore\": <integer 1-10>,\n" +
            "  \"feedback\": [\"<specific feedback point 1>\", \"<specific feedback point 2>\", \"<specific feedback point 3>\"],\n" +
            "  \"improvementAreas\": [\"<area to improve 1>\", \"<area to improve 2>\"],\n" +
            "  \"suggestedFollowUps\": [\"<follow-up question 1>\", \"<follow-up question 2>\"]\n" +
            "}";

        try
        {
            var result = await _aiProvider.GenerateStructuredResponseAsync<GeminiEvaluationResponse>(prompt, ct);
            if (result is null) return;

            var evaluation = new AnswerEvaluation
            {
                OverallScore = result.OverallScore,
                TechnicalScore = result.TechnicalScore,
                CommunicationScore = result.CommunicationScore,
                ConfidenceScore = result.ConfidenceScore,
                ClarityScore = result.ClarityScore,
                ProblemSolvingScore = result.ProblemSolvingScore,
                CompletenessScore = result.CompletenessScore,
                Feedback = result.Feedback,
                ImprovementAreas = result.ImprovementAreas,
                SuggestedFollowUps = result.SuggestedFollowUps
            };

            await _answerRepo.UpdateEvaluationAsync(answer.Id, evaluation, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to evaluate answer {AnswerId}", answer.Id);
        }
    }

    private static AnswerDto MapToDto(Answer a) => new(
        a.Id, a.QuestionId, a.SessionId, a.AnswerText,
        a.IsVoiceAnswer, a.DurationSeconds,
        a.Evaluation is null ? null : MapEvaluationToDto(a.Evaluation),
        a.SubmittedAt
    );

    private static AnswerEvaluationDto MapEvaluationToDto(AnswerEvaluation e) => new(
        e.OverallScore, e.TechnicalScore, e.CommunicationScore, e.ConfidenceScore,
        e.ClarityScore, e.ProblemSolvingScore, e.CompletenessScore,
        e.Feedback, e.ImprovementAreas, e.SuggestedFollowUps
    );
}
