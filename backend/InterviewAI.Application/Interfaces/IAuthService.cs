using InterviewAI.Application.DTOs.Auth;

namespace InterviewAI.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request, string? ipAddress = null, CancellationToken ct = default);
    Task<AuthResponse> LoginAsync(LoginRequest request, string? ipAddress = null, CancellationToken ct = default);
    Task<AuthResponse> RefreshTokenAsync(string refreshToken, string? ipAddress = null, CancellationToken ct = default);
    Task LogoutAsync(string userId, string refreshToken, CancellationToken ct = default);
    Task ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken ct = default);
    Task ResetPasswordAsync(ResetPasswordRequest request, CancellationToken ct = default);
    Task ChangePasswordAsync(string userId, ChangePasswordRequest request, CancellationToken ct = default);
    Task<UserDto> GetProfileAsync(string userId, CancellationToken ct = default);
}

public interface IJwtService
{
    string GenerateAccessToken(string userId, string email, string role);
    (string token, string jwtId) GenerateRefreshTokenPair();
    string? ValidateTokenAndGetUserId(string token);
    string? GetJwtIdFromToken(string token);
    DateTime GetAccessTokenExpiry();
}
