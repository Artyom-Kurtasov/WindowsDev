using WindowsDev.Domain.Entities;

namespace WindowsDev.Application.Identity.Repositories;

public interface IRefreshTokenRepository
{
    Task AddAsync(UserRefreshToken refreshToken);
    Task DeleteAsync(int refreshTokenId);
    Task UpdateAsync(UserRefreshToken refreshToken);
}
