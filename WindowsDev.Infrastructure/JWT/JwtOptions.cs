namespace WindowsDev.Infrastructure.JWT;

internal class JwtOptions
{
    public string Issuer { get; init; } = string.Empty;
    public string Audience { get; init; } = string.Empty;
    public string Key { get; init; } = string.Empty;
    public int ExpirationMinutes { get; init; }
    public int ExpirationRefreshTokenDays { get; init; }
    public int BytesSizeRefreshToken { get; init; }
}
