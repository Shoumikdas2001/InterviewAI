using InterviewAI.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace InterviewAI.Api.Controllers;

[ApiController]
[Route("api/resume")]
[Authorize]
public class ResumeController : ControllerBase
{
    private readonly IResumeService _resumeService;
    public ResumeController(IResumeService resumeService) => _resumeService = resumeService;

    private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? User.FindFirstValue("sub") ?? throw new UnauthorizedAccessException();

    [HttpPost("upload")]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<IActionResult> Upload(IFormFile file, CancellationToken ct)
    {
        var result = await _resumeService.UploadResumeAsync(GetUserId(), file, ct);
        return Ok(new { success = true, data = result });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetResume(string id, CancellationToken ct)
    {
        var resume = await _resumeService.GetResumeAsync(id, GetUserId(), ct);
        if (resume is null) return NotFound(new { success = false, message = "Resume not found." });
        return Ok(new { success = true, data = resume });
    }

    [HttpGet]
    public async Task<IActionResult> GetMyResumes(CancellationToken ct)
    {
        var resumes = await _resumeService.GetUserResumesAsync(GetUserId(), ct);
        return Ok(new { success = true, data = resumes });
    }

    [HttpPost("{id}/analyze")]
    public async Task<IActionResult> Analyze(string id, CancellationToken ct)
    {
        var result = await _resumeService.AnalyzeResumeAsync(id, GetUserId(), ct);
        return Ok(new { success = true, data = result });
    }

    [HttpGet("{id}/download")]
    public async Task<IActionResult> Download(string id, CancellationToken ct)
    {
        var bytes = await _resumeService.GetResumeFileAsync(id, GetUserId(), ct);
        return File(bytes, "application/pdf", "resume.pdf");
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id, CancellationToken ct)
    {
        await _resumeService.DeleteResumeAsync(id, GetUserId(), ct);
        return Ok(new { success = true, message = "Resume deleted." });
    }
}
