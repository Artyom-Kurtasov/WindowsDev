using WindowsDev.Domain.Entities;

namespace WindowsDev.Contracts.UI.Identity;

public record RegisterInfo(int RecoveryCode, string JwtToken, UserRefreshToken RefreshToken);
