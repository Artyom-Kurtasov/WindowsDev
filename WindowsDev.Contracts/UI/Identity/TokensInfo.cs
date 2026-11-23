using WindowsDev.Domain.Entities;

namespace WindowsDev.Contracts.UI.Identity;

public record TokensInfo(string AccessToken, UserRefreshToken RefreshToken);
