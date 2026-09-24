using Microsoft.EntityFrameworkCore;
using WindowsDev.Application.Identity;
using WindowsDev.Domain.Entities;
using WindowsDev.Infrastructure.Database.Interfaces;

namespace WindowsDev.Infrastructure.Repositories;

internal class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly IDbCreator _dbCreator;

    public RefreshTokenRepository(IDbCreator dbCreator)
    {
        _dbCreator = dbCreator;
    }

    public async Task AddAsync(UserRefreshToken refreshToken)
    {
        using var dbContext = _dbCreator.Create();

        await dbContext.RefreshToken.AddAsync(refreshToken);
        await dbContext.SaveChangesAsync();
    }

    public async Task DeleteAsync(int refreshTokenId)
    {
        using var dbContext = _dbCreator.Create();

        var refreshToken = new UserRefreshToken { Id = refreshTokenId };
        dbContext.RefreshToken.Attach(refreshToken);
        dbContext.RefreshToken.Remove(refreshToken);

        await dbContext.SaveChangesAsync();
    }

    public async Task UpdateAsync(UserRefreshToken refreshToken)
    {
        using var dbContext = _dbCreator.Create();

        var existingRefreshToken = await dbContext.RefreshToken.FirstOrDefaultAsync(x => x.Id == refreshToken.Id);

        if (existingRefreshToken is not null)
        {
            existingRefreshToken.TokenHash = refreshToken.TokenHash;
            existingRefreshToken.ExpiresAt = refreshToken.ExpiresAt;

            await dbContext.SaveChangesAsync();
        }
    }

    //public async Task<UserRefreshToken> GetByUserIdAsync(int userId)
    //{

    //}
}
