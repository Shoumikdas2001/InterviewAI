using InterviewAI.Application.DTOs.Interview;
using InterviewAI.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace InterviewAI.Api.Controllers;

[ApiController]
[Route("api/interview")]
[Authorize]
public class InterviewController : ControllerBase
{
    private readonly IInterviewService _interviewService;
    private readonly IQuestionService _questionService;
    private readonly IAnswerService _answerService;
    private readonly IAnalysisService _analysisService;

    public InterviewController(
        IInterviewService interviewService,
        IQuestionService questionService,
        IAnswerService answerService,
        IAnalysisService analysisService)
    {
        _interviewService = interviewService;
        _questionService = questionService;
        _answerService = answerService;
        _analysisService = analysisService;
    }

    private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? User.FindFirstValue("sub") ?? throw new UnauthorizedAccessException();

    // --- Session Endpoints ---

    [HttpPost("create")]
    public async Task<IActionResult> Create([FromBody] CreateInterviewRequest request, CancellationToken ct)
    {
        var result = await _interviewService.CreateInterviewAsync(GetUserId(), request, ct);
        return Ok(new { success = true, data = result });
    }

    [HttpGet("history")]
    public async Task<IActionResult> GetHistory(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        var sessions = await _interviewService.GetHistoryAsync(GetUserId(), page, pageSize, ct);
        return Ok(new { success = true, data = sessions });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetSession(string id, CancellationToken ct)
    {
        var session = await _interviewService.GetSessionAsync(id, GetUserId(), ct);
        if (session is null) return NotFound(new { success = false, message = "Session not found." });
        return Ok(new { success = true, data = session });
    }

    [HttpPost("{id}/start")]
    public async Task<IActionResult> Start(string id, CancellationToken ct)
    {
        await _interviewService.StartSessionAsync(id, GetUserId(), ct);
        return Ok(new { success = true, message = "Interview started." });
    }

    [HttpPost("{id}/complete")]
    public async Task<IActionResult> Complete(string id, CancellationToken ct)
    {
        await _interviewService.CompleteSessionAsync(id, GetUserId(), ct);
        return Ok(new { success = true, message = "Interview completed." });
    }

    // --- Question Endpoints ---

    [HttpGet("{id}/questions")]
    public async Task<IActionResult> GetQuestions(string id, CancellationToken ct)
    {
        var questions = await _questionService.GetSessionQuestionsAsync(id, GetUserId(), ct);
        return Ok(new { success = true, data = questions });
    }

    [HttpPost("questions/followup")]
    public async Task<IActionResult> GenerateFollowUp(
        [FromBody] GenerateFollowUpRequest request, CancellationToken ct)
    {
        var followUps = await _questionService.GenerateFollowUpQuestionsAsync(request, GetUserId(), ct);
        return Ok(new { success = true, data = followUps });
    }

    // --- Answer Endpoints ---

    [HttpPost("answers/submit")]
    public async Task<IActionResult> SubmitAnswer([FromBody] SubmitAnswerRequest request, CancellationToken ct)
    {
        var answer = await _answerService.SubmitAnswerAsync(GetUserId(), request, ct);
        return Ok(new { success = true, data = answer });
    }

    [HttpGet("{sessionId}/answers")]
    public async Task<IActionResult> GetAnswers(string sessionId, CancellationToken ct)
    {
        var answers = await _answerService.GetSessionAnswersAsync(sessionId, GetUserId(), ct);
        return Ok(new { success = true, data = answers });
    }

    // --- Analysis Endpoints ---

    [HttpPost("{id}/skill-gap")]
    public async Task<IActionResult> GetSkillGap(string id, CancellationToken ct)
    {
        var analysis = await _analysisService.GenerateSkillGapAnalysisAsync(id, GetUserId(), ct);
        return Ok(new { success = true, data = analysis });
    }

    [HttpPost("roadmap/generate")]
    public async Task<IActionResult> GenerateRoadmap([FromBody] GenerateRoadmapRequest request, CancellationToken ct)
    {
        var roadmap = await _analysisService.GenerateRoadmapAsync(GetUserId(), request, ct);
        return Ok(new { success = true, data = roadmap });
    }

    [HttpGet("roadmap/latest")]
    public async Task<IActionResult> GetLatestRoadmap(CancellationToken ct)
    {
        var roadmap = await _analysisService.GetLatestRoadmapAsync(GetUserId(), ct);
        if (roadmap is null) return NotFound(new { success = false, message = "No roadmap found." });
        return Ok(new { success = true, data = roadmap });
    }
}
