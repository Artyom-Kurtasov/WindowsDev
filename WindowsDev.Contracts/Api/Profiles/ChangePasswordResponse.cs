namespace WindowsDev.Contracts.Api.Profiles;

public class ChangePasswordResponse
{
    public int RecoveryCode { get; set; }
    public string JwtToken { get; set; } = string.Empty;
}