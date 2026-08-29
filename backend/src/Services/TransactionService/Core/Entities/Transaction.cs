using System;

namespace EasyTrack.TransactionService.Core.Entities;

public class Transaction
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid CategoryId { get; set; }
    public decimal Amount { get; set; }
    public string Type { get; set; } = string.Empty; // "income" or "expense"
    public string? Description { get; set; }
    public DateTime TransactionDate { get; set; }
    public string? SlipUrl { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public Category? Category { get; set; }
}
