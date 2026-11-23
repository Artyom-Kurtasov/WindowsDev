using WindowsDev.Domain.Entities;

namespace WindowsDev.Contracts.Api.Identity;

public class UserLoginResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public UserRefreshToken RefreshToken { get; set; }
}
