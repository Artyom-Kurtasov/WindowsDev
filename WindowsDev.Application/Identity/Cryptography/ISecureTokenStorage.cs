using WindowsDev.Domain.Entities;

namespace WindowsDev.Application.Identity.Cryptography;

public interface ISecureTokenStorage
{
    string? AccessToken { get; set; }
    UserRefreshToken? RefreshToken { get; set; }
    bool IsAuthenticated => !string.IsNullOrEmpty(AccessToken);
    void Clear();
}
