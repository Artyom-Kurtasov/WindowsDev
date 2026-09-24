using WindowsDev.Domain.Entities;

namespace WindowsDev.Application.Identity;

public interface IRefreshTokenRepository
{
    Task AddAsync(UserRefreshToken refreshToken);
    Task DeleteAsync(int refreshTokenId);
    Task UpdateAsync(UserRefreshToken refreshToken);
}
