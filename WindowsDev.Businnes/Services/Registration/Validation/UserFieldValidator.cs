using Microsoft.EntityFrameworkCore;
using WindowsDev.Businnes.DataBase;

namespace WindowsDev.Businnes.Services.Registration.Validation
{
    public class UserFieldValidator
    {
        private readonly AppDbContext _appDbContext;

        public UserFieldValidator(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public async Task<string> IsLoginTakenAsync(string login)
        {
            if (await _appDbContext.UsersInfo.AnyAsync(u => u.Login == login))
            {
                return "User with this login already exist";
            }

            return "Login is available";
        }

        public async Task<string> IsUsernameTakenAsync(string username)
        {
            if (await _appDbContext.UsersInfo.AnyAsync(u => u.Username == username))
            {
                return "Username already taken";
            }

            return "Username is available";
        }
    }
}
