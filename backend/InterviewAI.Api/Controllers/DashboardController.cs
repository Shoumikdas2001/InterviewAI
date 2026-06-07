using InterviewAI.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace InterviewAI.Api.Controllers;

[ApiController]
[Route("api/dashboard")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;
    public DashboardController(IDashboardService dashboardService) => _dashboardService = dashboardService;

    private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? User.FindFirstValue("sub") ?? throw new UnauthorizedAccessException();

    [HttpGet("analytics")]
    public async Task<IActionResult> GetAnalytics(CancellationToken ct)
    {
        var dashboard = await _dashboardService.GetDashboardAsync(GetUserId(), ct);
        return Ok(new { success = true, data = dashboard });
    }

    [HttpGet("admin")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> GetAdminDashboard(CancellationToken ct)
    {
        var dashboard = await _dashboardService.GetAdminDashboardAsync(ct);
        return Ok(new { success = true, data = dashboard });
    }
}

[ApiController]
[Route("api/reports")]
[Authorize]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;
    public ReportsController(IReportService reportService) => _reportService = reportService;

    private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? User.FindFirstValue("sub") ?? throw new UnauthorizedAccessException();

    [HttpPost("{sessionId}/generate")]
    public async Task<IActionResult> GenerateReport(string sessionId, CancellationToken ct)
    {
        var pdfBytes = await _reportService.GenerateReportAsync(sessionId, GetUserId(), ct);
        return File(pdfBytes, "application/pdf",
            $"InterviewAI-Report-{DateTime.UtcNow:yyyy-MM-dd}.pdf");
    }

    [HttpGet("{reportId}")]
    public async Task<IActionResult> GetReport(string reportId, CancellationToken ct)
    {
        var pdfBytes = await _reportService.GetReportAsync(reportId, GetUserId(), ct);
        return File(pdfBytes, "application/pdf", "interview-report.pdf");
    }
}

[ApiController]
[Route("api/admin")]
[Authorize(Policy = "AdminOnly")]
public class AdminController : ControllerBase
{
    private readonly IUserRepository _userRepository;

    public AdminController(IUserRepository userRepository) => _userRepository = userRepository;

    [HttpGet("users")]
    public async Task<IActionResult> GetUsers(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var users = await _userRepository.GetPagedAsync(page, pageSize, ct);
        return Ok(new { success = true, data = users });
    }

    [HttpPatch("users/{id}/toggle-status")]
    public async Task<IActionResult> ToggleUserStatus(string id, CancellationToken ct)
    {
        var user = await _userRepository.GetByIdAsync(id, ct);
        if (user is null) return NotFound(new { success = false, message = "User not found." });

        user.IsActive = !user.IsActive;
        user.UpdatedAt = DateTime.UtcNow;
        await _userRepository.UpdateAsync(id, user, ct);

        return Ok(new
        {
            success = true,
            message = $"User {(user.IsActive ? "enabled" : "disabled")} successfully.",
            isActive = user.IsActive
        });
    }
}
