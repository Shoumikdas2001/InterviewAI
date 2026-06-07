using System.ComponentModel.DataAnnotations;

namespace InterviewAI.Application.DTOs.Auth;

public record RegisterRequest(
    [Required] string FirstName,
    [Required] string LastName,
    [Required, EmailAddress] string Email,
    [Required, MinLength(8)] string Password
);

public record LoginRequest(
    [Required, EmailAddress] string Email,
    [Required] string Password
);

public record RefreshTokenRequest(
    [Required] string RefreshToken
);

public record ForgotPasswordRequest(
    [Required, EmailAddress] string Email
);

public record ResetPasswordRequest(
    [Required] string Token,
    [Required, EmailAddress] string Email,
    [Required, MinLength(8)] string NewPassword
);

public record ChangePasswordRequest(
    [Required] string CurrentPassword,
    [Required, MinLength(8)] string NewPassword
);

public record AuthResponse(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt,
    UserDto User
);

public record UserDto(
    string Id,
    string FirstName,
    string LastName,
    string Email,
    string Role,
    bool IsActive,
    int TotalInterviews,
    double AverageScore,
    DateTime CreatedAt
);
