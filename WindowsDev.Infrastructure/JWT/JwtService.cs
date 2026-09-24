using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using WindowsDev.Application.Identity;
using WindowsDev.Application.Users;
using WindowsDev.Domain.Entities;

namespace WindowsDev.Infrastructure.JWT
{
    internal class JwtService : IJwtService
    {
        private readonly JwtOptions _jwtOptions;
        private readonly IUserRepository _userRepository;
        private readonly IRefreshTokenRepository _refreshTokenRepository;

        public JwtService(IOptions<JwtOptions> jwtOptions, IUserRepository userRepository, IRefreshTokenRepository refreshTokenRepository)
        {
            _jwtOptions = jwtOptions.Value;
            _userRepository = userRepository;
            _refreshTokenRepository = refreshTokenRepository;
        }

        public string GenerateToken(int id, string login, string username)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, id.ToString()),
                new Claim(JwtRegisteredClaimNames.UniqueName, login),
                new Claim(JwtRegisteredClaimNames.Nickname, username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(
                    JwtRegisteredClaimNames.Iat,
                    new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString(),
                    ClaimValueTypes.Integer64
                ),
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.Key));

            var token = new JwtSecurityToken(
                issuer: _jwtOptions.Issuer,
                audience: _jwtOptions.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwtOptions.ExpirationMinutes),
                signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public UserRefreshToken GenerateRefreshToken(int userId)
        {
            return new UserRefreshToken
            {
                UserId = userId,
                ExpiresAt = DateTime.UtcNow.AddDays(_jwtOptions.ExpirationRefreshTokenDays),
                TokenHash = Convert.ToBase64String(RandomNumberGenerator.GetBytes(_jwtOptions.BytesSizeRefreshToken))
            };
        }
    }
}
