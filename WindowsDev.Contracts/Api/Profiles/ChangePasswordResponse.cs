namespace WindowsDev.Contracts.Api.Profiles;

public class ChangePasswordResponse
{
    public int RecoveryCode { get; set; }
    public string Token { get; set; } = string.Empty;
}