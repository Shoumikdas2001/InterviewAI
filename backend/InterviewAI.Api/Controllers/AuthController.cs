using InterviewAI.Application.DTOs.Auth;
using InterviewAI.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace InterviewAI.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService) => _authService = authService;

    private string? GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? User.FindFirstValue("sub");
    private string? GetIpAddress() => HttpContext.Connection.RemoteIpAddress?.ToString();

    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponse), 200)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken ct)
    {
        var result = await _authService.RegisterAsync(request, GetIpAddress(), ct);
        return Ok(new { success = true, data = result });
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponse), 200)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        var result = await _authService.LoginAsync(request, GetIpAddress(), ct);
        return Ok(new { success = true, data = result });
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request, CancellationToken ct)
    {
        var result = await _authService.RefreshTokenAsync(request.RefreshToken, GetIpAddress(), ct);
        return Ok(new { success = true, data = result });
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout([FromBody] RefreshTokenRequest request, CancellationToken ct)
    {
        await _authService.LogoutAsync(GetUserId()!, request.RefreshToken, ct);
        return Ok(new { success = true, message = "Logged out successfully." });
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request, CancellationToken ct)
    {
        await _authService.ForgotPasswordAsync(request, ct);
        return Ok(new { success = true, message = "If that email exists, a reset link has been sent." });
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request, CancellationToken ct)
    {
        await _authService.ResetPasswordAsync(request, ct);
        return Ok(new { success = true, message = "Password reset successfully." });
    }

    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request, CancellationToken ct)
    {
        await _authService.ChangePasswordAsync(GetUserId()!, request, ct);
        return Ok(new { success = true, message = "Password changed successfully." });
    }

    [HttpGet("profile")]
    [Authorize]
    public async Task<IActionResult> GetProfile(CancellationToken ct)
    {
        var user = await _authService.GetProfileAsync(GetUserId()!, ct);
        return Ok(new { success = true, data = user });
    }
}
