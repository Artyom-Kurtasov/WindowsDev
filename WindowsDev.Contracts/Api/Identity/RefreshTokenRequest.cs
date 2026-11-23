using WindowsDev.Domain.Entities;

namespace WindowsDev.Contracts.Api.Identity;

public class RefreshTokenRequest
{
    public required UserRefreshToken RefreshToken { get; set; }
}
