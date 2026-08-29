using EasyTrack.TransactionService.Core.Entities;
using EasyTrack.TransactionService.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace EasyTrack.TransactionService.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/transactions")]
public class TransactionsController : ControllerBase
{
    private readonly TransactionDbContext _context;

    public TransactionsController(TransactionDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetTransactions(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] string? type = null,
        [FromQuery] Guid? categoryId = null)
    {
        var userId = GetUserId();
        var query = _context.Transactions
            .Include(t => t.Category)
            .Where(t => t.UserId == userId);

        if (startDate.HasValue)
        {
            var startUtc = DateTime.SpecifyKind(startDate.Value.Date, DateTimeKind.Utc);
            query = query.Where(t => t.TransactionDate >= startUtc);
        }

        if (endDate.HasValue)
        {
            var endUtc = DateTime.SpecifyKind(endDate.Value.Date, DateTimeKind.Utc).AddDays(1).AddTicks(-1);
            query = query.Where(t => t.TransactionDate <= endUtc);
        }

        if (!string.IsNullOrEmpty(type))
        {
            query = query.Where(t => t.Type == type);
        }

        if (categoryId.HasValue)
        {
            query = query.Where(t => t.CategoryId == categoryId.Value);
        }

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(t => t.TransactionDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(t => new TransactionDto(
                t.Id,
                t.UserId,
                t.Amount,
                t.Type,
                t.Description,
                t.TransactionDate,
                t.SlipUrl,
                new CategoryDto(t.Category!.Id, t.Category.UserId, t.Category.Name, t.Category.Type, t.Category.IconName, t.Category.ColorHex)
            ))
            .ToListAsync();

        return Ok(new PagedResult<TransactionDto>(items, page, pageSize, totalCount));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetTransaction(Guid id)
    {
        var userId = GetUserId();
        var transaction = await _context.Transactions
            .Include(t => t.Category)
            .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

        if (transaction == null)
        {
            return NotFound();
        }

        return Ok(new TransactionDto(
            transaction.Id,
            transaction.UserId,
            transaction.Amount,
            transaction.Type,
            transaction.Description,
            transaction.TransactionDate,
            transaction.SlipUrl,
            new CategoryDto(transaction.Category!.Id, transaction.Category.UserId, transaction.Category.Name, transaction.Category.Type, transaction.Category.IconName, transaction.Category.ColorHex)
        ));
    }

    [HttpPost]
    public async Task<IActionResult> CreateTransaction([FromBody] TransactionRequest request)
    {
        var userId = GetUserId();

        // Validate category
        var category = await _context.Categories
            .FirstOrDefaultAsync(c => c.Id == request.CategoryId && (c.UserId == null || c.UserId == userId));

        if (category == null)
        {
            return BadRequest(new { message = "Invalid category." });
        }

        var transaction = new Transaction
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            CategoryId = request.CategoryId,
            Amount = request.Amount,
            Type = request.Type,
            Description = request.Description,
            TransactionDate = request.TransactionDate.ToUniversalTime(),
            SlipUrl = request.SlipUrl,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync();

        // Reload to include category details in response
        transaction.Category = category;

        return CreatedAtAction(nameof(GetTransaction), new { id = transaction.Id }, new TransactionDto(
            transaction.Id,
            transaction.UserId,
            transaction.Amount,
            transaction.Type,
            transaction.Description,
            transaction.TransactionDate,
            transaction.SlipUrl,
            new CategoryDto(category.Id, category.UserId, category.Name, category.Type, category.IconName, category.ColorHex)
        ));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateTransaction(Guid id, [FromBody] TransactionRequest request)
    {
        var userId = GetUserId();
        var transaction = await _context.Transactions
            .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

        if (transaction == null)
        {
            return NotFound();
        }

        // Validate category
        var category = await _context.Categories
            .FirstOrDefaultAsync(c => c.Id == request.CategoryId && (c.UserId == null || c.UserId == userId));

        if (category == null)
        {
            return BadRequest(new { message = "Invalid category." });
        }

        transaction.Amount = request.Amount;
        transaction.Type = request.Type;
        transaction.CategoryId = request.CategoryId;
        transaction.Description = request.Description;
        transaction.TransactionDate = request.TransactionDate.ToUniversalTime();
        transaction.SlipUrl = request.SlipUrl;
        transaction.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new TransactionDto(
            transaction.Id,
            transaction.UserId,
            transaction.Amount,
            transaction.Type,
            transaction.Description,
            transaction.TransactionDate,
            transaction.SlipUrl,
            new CategoryDto(category.Id, category.UserId, category.Name, category.Type, category.IconName, category.ColorHex)
        ));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTransaction(Guid id)
    {
        var userId = GetUserId();
        var transaction = await _context.Transactions
            .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

        if (transaction == null)
        {
            return NotFound();
        }

        _context.Transactions.Remove(transaction);
        await _context.SaveChangesAsync();

        return NoContent();
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
