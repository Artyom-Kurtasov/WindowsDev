namespace WindowsDev.Api.DTO.Response.ProfilesController;

public class ProfileChangePasswordResponse
{
    public int RecoveryCode { get; set; }
    public string JwtToken { get; set; } = string.Empty;
}