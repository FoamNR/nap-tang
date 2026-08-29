using System;
using System.ComponentModel.DataAnnotations;

namespace EasyTrack.AuthService.Controllers;

public record RegisterRequest(
    [Required, EmailAddress] string Email,
    [Required, MinLength(8)] string Password,
    [Required] string ConfirmPassword,
    [Required, MinLength(2), MaxLength(100)] string DisplayName
);

public record LoginRequest(
    [Required, EmailAddress] string Email,
    [Required] string Password
);

public record UserDto(
    Guid Id,
    string Email,
    string DisplayName
);

public record AuthResponse(
    string AccessToken,
    int ExpiresInSeconds,
    UserDto User
);

public record UpdateProfileRequest(
    [Required, MinLength(2), MaxLength(100)] string DisplayName,
    [MinLength(8)] string? NewPassword,
    string? ConfirmPassword
);
