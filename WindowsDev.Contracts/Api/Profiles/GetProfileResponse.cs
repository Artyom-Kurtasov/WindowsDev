namespace WindowsDev.Contracts.Api.Profiles;

public class GetProfileResponse
{
    public int Id { get; set; }
    public string Login { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
}
