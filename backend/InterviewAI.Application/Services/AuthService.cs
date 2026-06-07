using BCrypt.Net;
using InterviewAI.Application.DTOs.Auth;
using InterviewAI.Application.Interfaces;
using InterviewAI.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace InterviewAI.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly IJwtService _jwtService;
    private readonly ILogger<AuthService> _logger;
    private readonly int _refreshTokenExpiryDays;

    public AuthService(
        IUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IAuditLogRepository auditLogRepository,
        IJwtService jwtService,
        IConfiguration configuration,
        ILogger<AuthService> logger)
    {
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _auditLogRepository = auditLogRepository;
        _jwtService = jwtService;
        _logger = logger;
        _refreshTokenExpiryDays = int.TryParse(configuration["Jwt:RefreshTokenExpiryDays"], out var d) ? d : 30;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, string? ipAddress = null, CancellationToken ct = default)
    {
        var email = request.Email.ToLowerInvariant();

        if (await _userRepository.ExistsAsync(u => u.Email == email, ct))
            throw new InvalidOperationException("An account with this email already exists.");

        var user = new User
        {
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password, workFactor: 12)
        };

        await _userRepository.CreateAsync(user, ct);

        await _auditLogRepository.LogAsync(user.Id, "Register", "User", user.Id,
            $"New user registered: {email}", ipAddress, ct: ct);

        _logger.LogInformation("New user registered: {Email}", email);

        return await BuildAuthResponseAsync(user, ipAddress, ct);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, string? ipAddress = null, CancellationToken ct = default)
    {
        var email = request.Email.ToLowerInvariant();
        var user = await _userRepository.GetByEmailAsync(email, ct)
            ?? throw new UnauthorizedAccessException("Invalid email or password.");

        if (!user.IsActive)
            throw new UnauthorizedAccessException("Your account has been disabled. Please contact support.");

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            await _auditLogRepository.LogAsync(user.Id, "Login", "User", user.Id,
                "Failed login attempt", ipAddress, isSuccess: false, ct: ct);
            throw new UnauthorizedAccessException("Invalid email or password.");
        }

        await _userRepository.UpdateLastLoginAsync(user.Id, ct);
        await _auditLogRepository.LogAsync(user.Id, "Login", "User", user.Id,
            "Successful login", ipAddress, ct: ct);

        return await BuildAuthResponseAsync(user, ipAddress, ct);
    }

    public async Task<AuthResponse> RefreshTokenAsync(string refreshToken, string? ipAddress = null, CancellationToken ct = default)
    {
        var storedToken = await _refreshTokenRepository.GetByTokenAsync(refreshToken, ct)
            ?? throw new UnauthorizedAccessException("Invalid refresh token.");

        if (storedToken.IsRevoked)
            throw new UnauthorizedAccessException("Refresh token has been revoked.");

        if (storedToken.IsUsed)
            throw new UnauthorizedAccessException("Refresh token has already been used.");

        if (storedToken.ExpiresAt < DateTime.UtcNow)
            throw new UnauthorizedAccessException("Refresh token has expired.");

        await _refreshTokenRepository.MarkAsUsedAsync(storedToken.Id, ct);

        var user = await _userRepository.GetByIdAsync(storedToken.UserId, ct)
            ?? throw new UnauthorizedAccessException("User not found.");

        if (!user.IsActive)
            throw new UnauthorizedAccessException("Account is disabled.");

        return await BuildAuthResponseAsync(user, ipAddress, ct);
    }

    public async Task LogoutAsync(string userId, string refreshToken, CancellationToken ct = default)
    {
        var storedToken = await _refreshTokenRepository.GetByTokenAsync(refreshToken, ct);
        if (storedToken is not null)
            await _refreshTokenRepository.MarkAsUsedAsync(storedToken.Id, ct);

        await _auditLogRepository.LogAsync(userId, "Logout", "User", userId, ct: ct);
    }

    public async Task ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken ct = default)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email.ToLowerInvariant(), ct);
        if (user is null) return; // Silent fail — don't reveal if email exists

        var token = Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(32));
        user.PasswordResetToken = token;
        user.PasswordResetExpiry = DateTime.UtcNow.AddHours(1);
        user.UpdatedAt = DateTime.UtcNow;
        await _userRepository.UpdateAsync(user.Id, user, ct);

        // In production: send email via IEmailService
        // For now, log the token (remove in prod)
        _logger.LogWarning("Password reset token for {Email}: {Token}", request.Email, token);
    }

    public async Task ResetPasswordAsync(ResetPasswordRequest request, CancellationToken ct = default)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email.ToLowerInvariant(), ct)
            ?? throw new InvalidOperationException("Invalid reset request.");

        if (user.PasswordResetToken != request.Token || user.PasswordResetExpiry < DateTime.UtcNow)
            throw new InvalidOperationException("Invalid or expired reset token.");

        var newHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword, workFactor: 12);
        await _userRepository.UpdatePasswordAsync(user.Id, newHash, ct);
    }

    public async Task ChangePasswordAsync(string userId, ChangePasswordRequest request, CancellationToken ct = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, ct)
            ?? throw new InvalidOperationException("User not found.");

        if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
            throw new UnauthorizedAccessException("Current password is incorrect.");

        var newHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword, workFactor: 12);
        await _userRepository.UpdatePasswordAsync(userId, newHash, ct);
    }

    public async Task<UserDto> GetProfileAsync(string userId, CancellationToken ct = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, ct)
            ?? throw new InvalidOperationException("User not found.");
        return MapToUserDto(user);
    }

    private async Task<AuthResponse> BuildAuthResponseAsync(User user, string? ipAddress, CancellationToken ct)
    {
        var accessToken = _jwtService.GenerateAccessToken(user.Id, user.Email, user.Role.ToString());
        var (refreshToken, jwtId) = _jwtService.GenerateRefreshTokenPair();

        var refreshTokenEntity = new RefreshToken
        {
            UserId = user.Id,
            Token = refreshToken,
            JwtId = jwtId,
            ExpiresAt = DateTime.UtcNow.AddDays(_refreshTokenExpiryDays),
            IpAddress = ipAddress
        };

        await _refreshTokenRepository.CreateAsync(refreshTokenEntity, ct);

        return new AuthResponse(
            AccessToken: accessToken,
            RefreshToken: refreshToken,
            ExpiresAt: _jwtService.GetAccessTokenExpiry(),
            User: MapToUserDto(user)
        );
    }

    private static UserDto MapToUserDto(User user) => new(
        user.Id,
        user.FirstName,
        user.LastName,
        user.Email,
        user.Role.ToString(),
        user.IsActive,
        user.TotalInterviews,
        user.AverageScore,
        user.CreatedAt
    );
}
