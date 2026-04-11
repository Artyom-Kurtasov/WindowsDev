using Microsoft.EntityFrameworkCore;
using WindowsDev.Business.DataBase;

namespace WindowsDev.Business.Services.Registration.Validation
{
    public class UserFieldValidator
    {
        private readonly DbManager _dbManager;

        public UserFieldValidator(DbManager dbManager)
        {
            _dbManager = dbManager;
        }

        public async Task<string> IsLoginTakenAsync(string login)
        {
            using var dbContext = _dbManager.Create();

            if (await dbContext.UsersInfo.AnyAsync(u => u.Login == login))
            {
                return "User with this login already exist";
            }

            return "Login is available";
        }

        public async Task<string> IsUsernameTakenAsync(string username)
        {
            using var dbContext = _dbManager.Create();
            if (await dbContext.UsersInfo.AnyAsync(u => u.Username == username))
            {
                return "Username already taken";
            }

            return "Username is available";
        }
    }
}

