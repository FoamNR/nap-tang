using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EasyTrack.TransactionService.Controllers;

public record CategoryRequest(
    [Required, MaxLength(100)] string Name,
    [Required, RegularExpression("^(income|expense)$")] string Type,
    [Required, MaxLength(50)] string IconName,
    [Required, RegularExpression("^#[0-9A-Fa-f]{6}$")] string ColorHex
);

public record CategoryDto(
    Guid Id,
    Guid? UserId,
    string Name,
    string Type,
    string IconName,
    string ColorHex
);

public record TransactionRequest(
    [Required, Range(0.01, double.MaxValue)] decimal Amount,
    [Required, RegularExpression("^(income|expense)$")] string Type,
    [Required] Guid CategoryId,
    [MaxLength(1000)] string? Description,
    [Required] DateTime TransactionDate,
    [MaxLength(500)] string? SlipUrl
);

public record TransactionDto(
    Guid Id,
    Guid UserId,
    decimal Amount,
    string Type,
    string? Description,
    DateTime TransactionDate,
    string? SlipUrl,
    CategoryDto Category
);

public record PagedResult<T>(
    IEnumerable<T> Items,
    int Page,
    int PageSize,
    int TotalCount
);
