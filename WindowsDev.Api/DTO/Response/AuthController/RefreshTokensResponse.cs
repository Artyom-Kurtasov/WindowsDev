namespace WindowsDev.Api.DTO.Response.AuthController
{
    public class RefreshTokensResponse
    {
        public string RefreshToken { get; set; } = string.Empty;
        public string AccessToken { get; set; } = string.Empty;
    }
}
