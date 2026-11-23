namespace WindowsDev.Domain.Entities;

public class UserRefreshToken
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Token { get; set; } = string.Empty;
    public bool IsRevoked { get; set; } = false;
    public DateTime ExpiresAt { get; set; }
}
