namespace WindowsDev.Application.Identity;

public interface ISecureTokenStorage
{
    string? AccessToken { get; set; }
    bool IsAuthenticated => !string.IsNullOrEmpty(AccessToken);
    void Clear();
}
