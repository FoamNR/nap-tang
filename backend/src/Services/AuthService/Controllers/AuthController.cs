using EasyTrack.AuthService.Core.Entities;
using EasyTrack.AuthService.Core.Interfaces;
using EasyTrack.AuthService.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace EasyTrack.AuthService.Controllers;

[ApiController]
[Route("api/v1/auth")]
public class AuthController : ControllerBase
{
    private readonly AuthDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;

    public AuthController(
        AuthDbContext context,
        IPasswordHasher passwordHasher,
        ITokenService tokenService)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        if (request.Password != request.ConfirmPassword)
        {
            return BadRequest(new { message = "Passwords do not match." });
        }

        if (await _context.Users.AnyAsync(u => u.Email == request.Email))
        {
            return BadRequest(new { message = "Email is already in use." });
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            PasswordHash = _passwordHasher.HashPassword(request.Password),
            DisplayName = request.DisplayName,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(Register), new UserDto(user.Id, user.Email, user.DisplayName));
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
        if (user == null || !_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
        {
            return Unauthorized(new { message = "Invalid email or password." });
        }

        var accessToken = _tokenService.GenerateAccessToken(user);
        var refreshTokenString = _tokenService.GenerateRefreshToken();

        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = refreshTokenString,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow
        };

        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync();

        SetRefreshTokenCookie(refreshTokenString);

        return Ok(new AuthResponse(
            AccessToken: accessToken,
            ExpiresInSeconds: 900, // 15 mins
            User: new UserDto(user.Id, user.Email, user.DisplayName)
        ));
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh()
    {
        var refreshTokenString = Request.Cookies["refreshToken"];
        if (string.IsNullOrEmpty(refreshTokenString))
        {
            return Unauthorized(new { message = "Missing refresh token." });
        }

        var refreshToken = await _context.RefreshTokens
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Token == refreshTokenString);

        if (refreshToken == null || !refreshToken.IsActive)
        {
            return Unauthorized(new { message = "Invalid or expired refresh token." });
        }

        // Revoke the old refresh token
        refreshToken.IsRevoked = true;

        // Generate new tokens
        var user = refreshToken.User!;
        var newAccessToken = _tokenService.GenerateAccessToken(user);
        var newRefreshTokenString = _tokenService.GenerateRefreshToken();

        var newRefreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = newRefreshTokenString,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow
        };

        _context.RefreshTokens.Add(newRefreshToken);
        await _context.SaveChangesAsync();

        SetRefreshTokenCookie(newRefreshTokenString);

        return Ok(new AuthResponse(
            AccessToken: newAccessToken,
            ExpiresInSeconds: 900,
            User: new UserDto(user.Id, user.Email, user.DisplayName)
        ));
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var refreshTokenString = Request.Cookies["refreshToken"];
        if (!string.IsNullOrEmpty(refreshTokenString))
        {
            var refreshToken = await _context.RefreshTokens.FirstOrDefaultAsync(r => r.Token == refreshTokenString);
            if (refreshToken != null)
            {
                refreshToken.IsRevoked = true;
                await _context.SaveChangesAsync();
            }
        }

        Response.Cookies.Delete("refreshToken", new CookieOptions
        {
            HttpOnly = true,
            Secure = Request.IsHttps,
            SameSite = SameSiteMode.Lax
        });

        return NoContent();
    }

    private void SetRefreshTokenCookie(string token)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = Request.IsHttps,
            SameSite = SameSiteMode.Lax,
            Expires = DateTime.UtcNow.AddDays(7)
        };
        Response.Cookies.Append("refreshToken", token, cookieOptions);
    }

    [Authorize]
    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        Guid userId;
        try
        {
            userId = GetUserId();
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null)
        {
            return NotFound(new { message = "User not found." });
        }

        user.DisplayName = request.DisplayName;
        
        if (!string.IsNullOrEmpty(request.NewPassword))
        {
            if (request.NewPassword != request.ConfirmPassword)
            {
                return BadRequest(new { message = "Passwords do not match." });
            }
            user.PasswordHash = _passwordHasher.HashPassword(request.NewPassword);
        }

        user.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        // Re-generate access token with updated claims
        var accessToken = _tokenService.GenerateAccessToken(user);

        return Ok(new AuthResponse(
            AccessToken: accessToken,
            ExpiresInSeconds: 900,
            User: new UserDto(user.Id, user.Email, user.DisplayName)
        ));
    }

    private Guid GetUserId()
    {
        var subClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value 
            ?? User.FindFirst("sub")?.Value;
        if (string.IsNullOrEmpty(subClaim) || !Guid.TryParse(subClaim, out var userId))
        {
            throw new UnauthorizedAccessException("User is not authenticated.");
        }
        return userId;
    }
}
