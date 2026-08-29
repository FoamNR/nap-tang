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
[Route("api/v1/transactions/categories")]
public class CategoriesController : ControllerBase
{
    private readonly TransactionDbContext _context;

    public CategoriesController(TransactionDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetCategories()
    {
        var userId = GetUserId();
        var categories = await _context.Categories
            .Where(c => c.UserId == null || c.UserId == userId)
            .OrderBy(c => c.Name)
            .Select(c => new CategoryDto(c.Id, c.UserId, c.Name, c.Type, c.IconName, c.ColorHex))
            .ToListAsync();

        return Ok(categories);
    }

    [HttpPost]
    public async Task<IActionResult> CreateCategory([FromBody] CategoryRequest request)
    {
        var userId = GetUserId();

        if (await _context.Categories.AnyAsync(c => (c.UserId == null || c.UserId == userId) 
            && c.Name.ToLower() == request.Name.ToLower() 
            && c.Type == request.Type))
        {
            return BadRequest(new { message = "A category with this name already exists." });
        }

        var category = new Category
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = request.Name,
            Type = request.Type,
            IconName = request.IconName,
            ColorHex = request.ColorHex,
            CreatedAt = DateTime.UtcNow
        };

        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetCategories), new CategoryDto(category.Id, category.UserId, category.Name, category.Type, category.IconName, category.ColorHex));
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
