namespace WindowsDev.Contracts.Api.Identity
{
    public class RefreshTokensResponse
    {
        public string RefreshToken { get; set; } = string.Empty;
        public string AccessToken { get; set; } = string.Empty;
    }
}
