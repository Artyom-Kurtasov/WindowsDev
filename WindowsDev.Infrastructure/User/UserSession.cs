using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using WindowsDev.Application.Identity;
using WindowsDev.Application.Primitives;
using WindowsDev.Domain.Messages.DialogsMessages.Errors;

namespace WindowsDev.Infrastructure.User;

internal class UserSession : IUserSession
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserSession(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public int UserId => int.TryParse(GetRequiredClaim(ClaimTypes.NameIdentifier), out var userId) ? userId :
        throw new FormatException($"Claim {ClaimTypes.NameIdentifier} must contain an integer user id");
    public string Login => GetRequiredClaim(ClaimTypes.Name);
    public string Username => GetRequiredClaim(JwtRegisteredClaimNames.Nickname);

    private string GetRequiredClaim(string claimType)
    {
        var user = _httpContextAccessor.HttpContext?.User ??
            throw new InvalidOperationException("Current user is available only during an active HTTP request.");

        return user.FindFirst(claimType)?.Value ??
            throw new InvalidOperationException($"Required claim {claimType} is missing");
    }
}
