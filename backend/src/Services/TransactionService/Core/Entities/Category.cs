using System;

namespace EasyTrack.TransactionService.Core.Entities;

public class Category
{
    public Guid Id { get; set; }
    public Guid? UserId { get; set; } // Null = system default, not Null = custom user category
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // "income" or "expense"
    public string IconName { get; set; } = string.Empty; // Lucide icon name
    public string ColorHex { get; set; } = string.Empty; // Hex color string (e.g. #EF4444)
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
