using EasyTrack.AuthService.Core.Entities;
using System.Security.Claims;

namespace EasyTrack.AuthService.Core.Interfaces;

public interface ITokenService
{
    string GenerateAccessToken(User user);
    string GenerateRefreshToken();
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
}
