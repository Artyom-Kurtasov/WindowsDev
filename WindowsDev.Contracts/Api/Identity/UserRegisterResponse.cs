using WindowsDev.Domain.Entities;

namespace WindowsDev.Contracts.Api.Identity;

public record UserRegisterResponse(int RecoveryCode, string JwtToken, UserRefreshToken RefreshToken);
