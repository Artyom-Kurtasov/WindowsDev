using Microsoft.EntityFrameworkCore;
using WindowsDev.Business.DataBase.Interfaces;
using WindowsDev.Business.Repositories.Interfaces;
using WindowsDev.Domain.UsersModels;

namespace WindowsDev.Business.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly IDbManager _dbManager;

        public UserRepository(IDbManager dbManager)
        {
            _dbManager = dbManager;
        }

        public async Task AddAsync(UsersInfo user)
        {
            using var dbContext = _dbManager.Create();

            await dbContext.UsersInfo.AddAsync(user);
            await dbContext.SaveChangesAsync();
        }

        public async Task<bool> ExistsByLoginAsync(string login)
        {
            using var dbContext = _dbManager.Create();

            if (await dbContext.UsersInfo.AnyAsync(x => x.Login == login))
                return true;

            return false;
        }

        public async Task<bool> ExistsByUsernameAsync(string username)
        {
            using var dbContext = _dbManager.Create();

            if (await dbContext.UsersInfo.AnyAsync(x => x.Username == username))
                return true;

            return false;
        }

        public async Task<UsersInfo?> GetByLoginAsync(string login)
        {
            using var dbContext = _dbManager.Create();

            return await dbContext.UsersInfo.FirstOrDefaultAsync(x => x.Login == login);
        }

        public async Task UpdateAsync(UsersInfo user)
        {
            using var dbContext = _dbManager.Create();

            var existingUser = await dbContext.UsersInfo.FirstOrDefaultAsync(x =>
                x.Login == user.Login
            );

            if (existingUser != null)
            {
                existingUser.Login = user.Login;
                existingUser.Username = user.Username;
                existingUser.Salt = user.Salt;
                existingUser.HashMethod = user.HashMethod;
                existingUser.PasswordHash = user.PasswordHash;
                existingUser.RecoveryCodeHash = user.RecoveryCodeHash;
                existingUser.RecoveryCodeSalt = user.RecoveryCodeSalt;

                await dbContext.SaveChangesAsync();
            }
        }
    }
}
