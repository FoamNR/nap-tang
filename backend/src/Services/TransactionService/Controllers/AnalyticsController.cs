using EasyTrack.TransactionService.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace EasyTrack.TransactionService.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/transactions/analytics")]
public class AnalyticsController : ControllerBase
{
    private readonly TransactionDbContext _context;

    public AnalyticsController(TransactionDbContext context)
    {
        _context = context;
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        var userId = GetUserId();
        
        // Make sure dates are UTC-friendly for comparison
        var startUtc = DateTime.SpecifyKind(startDate.Date, DateTimeKind.Utc);
        var endUtc = DateTime.SpecifyKind(endDate.Date, DateTimeKind.Utc).AddDays(1).AddTicks(-1);

        var transactions = await _context.Transactions
            .Where(t => t.UserId == userId && t.TransactionDate >= startUtc && t.TransactionDate <= endUtc)
            .ToListAsync();

        var totalIncome = transactions.Where(t => t.Type == "income").Sum(t => t.Amount);
        var totalExpense = transactions.Where(t => t.Type == "expense").Sum(t => t.Amount);
        var netBalance = totalIncome - totalExpense;

        return Ok(new SummaryResponse(totalIncome, totalExpense, netBalance, startUtc, endUtc));
    }

    [HttpGet("category-breakdown")]
    public async Task<IActionResult> GetCategoryBreakdown(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        [FromQuery] string? type = null)
    {
        var userId = GetUserId();
        
        var startUtc = DateTime.SpecifyKind(startDate.Date, DateTimeKind.Utc);
        var endUtc = DateTime.SpecifyKind(endDate.Date, DateTimeKind.Utc).AddDays(1).AddTicks(-1);

        var query = _context.Transactions
            .Include(t => t.Category)
            .Where(t => t.UserId == userId && t.TransactionDate >= startUtc && t.TransactionDate <= endUtc);

        if (!string.IsNullOrEmpty(type))
        {
            query = query.Where(t => t.Type == type);
        }

        var transactions = await query.ToListAsync();
        var totalSum = transactions.Sum(t => t.Amount);

        if (totalSum == 0)
        {
            return Ok(new List<CategoryBreakdownDto>());
        }

        var breakdown = transactions
            .GroupBy(t => t.Category!)
            .Select(g => new CategoryBreakdownDto(
                CategoryId: g.Key.Id,
                CategoryName: g.Key.Name,
                IconName: g.Key.IconName,
                ColorHex: g.Key.ColorHex,
                TotalAmount: g.Sum(t => t.Amount),
                Percentage: Math.Round((g.Sum(t => t.Amount) / totalSum) * 100, 2)
            ))
            .OrderByDescending(b => b.TotalAmount)
            .ToList();

        return Ok(breakdown);
    }

    [HttpGet("trend")]
    public async Task<IActionResult> GetTrend(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        [FromQuery] string interval) // "daily", "weekly", "monthly"
    {
        var userId = GetUserId();
        
        var startUtc = DateTime.SpecifyKind(startDate.Date, DateTimeKind.Utc);
        var endUtc = DateTime.SpecifyKind(endDate.Date, DateTimeKind.Utc).AddDays(1).AddTicks(-1);

        var transactions = await _context.Transactions
            .Where(t => t.UserId == userId && t.TransactionDate >= startUtc && t.TransactionDate <= endUtc)
            .ToListAsync();

        IEnumerable<IGrouping<string, Core.Entities.Transaction>> grouped;

        switch (interval.ToLower())
        {
            case "daily":
                grouped = transactions.GroupBy(t => t.TransactionDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
                break;
            case "weekly":
                // Group by the start of the week (Monday)
                grouped = transactions.GroupBy(t =>
                {
                    var date = t.TransactionDate.Date;
                    int diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
                    return date.AddDays(-1 * diff).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                });
                break;
            case "monthly":
                grouped = transactions.GroupBy(t => t.TransactionDate.ToString("yyyy-MM", CultureInfo.InvariantCulture));
                break;
            default:
                return BadRequest(new { message = "Invalid interval. Allowed values: daily, weekly, monthly." });
        }

        var trendPoints = grouped
            .Select(g => new TrendDataPoint(
                Label: g.Key,
                Income: g.Where(t => t.Type == "income").Sum(t => t.Amount),
                Expense: g.Where(t => t.Type == "expense").Sum(t => t.Amount)
            ))
            .OrderBy(pt => pt.Label)
            .ToList();

        return Ok(new TrendResponse(interval.ToLower(), trendPoints));
    }

    private Guid GetUserId()
    {
        var subClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
            ?? User.FindFirst("sub")?.Value;
        if (string.IsNullOrEmpty(subClaim) || !Guid.TryParse(subClaim, out var userId))
        {
            throw new UnauthorizedAccessException("User is not authenticated.");
        }
        return userId;
    }
}
