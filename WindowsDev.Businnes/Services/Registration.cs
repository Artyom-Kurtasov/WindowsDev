using WindowsDev.Businnes.DataBase;
using WindowsDev.Domain.UsersAuthInfo;
using System.Linq;

namespace WindowsDev.Businnes.Services
{
    public class Registration
    {
        private readonly AppDbContext _appDbContext;
        private readonly UserAuthInfo _userAuthInfo;

        public Registration(AppDbContext appDbContext, UserAuthInfo userAuthInfo)
        {
            _appDbContext = appDbContext;
            _userAuthInfo = userAuthInfo;
        }
        public void Adds(string password, string email, string login)
        { 
            _userAuthInfo.PasswordHash = password;
            _userAuthInfo.Email = email;
            _userAuthInfo.Salt = "dfddfsw";

            if (!_appDbContext.UserAuthInfo
                .Any(x => x.Login == login))
            {
                _userAuthInfo.Login = login;

                _appDbContext.Add(_userAuthInfo);
                _appDbContext.SaveChanges();
            }
        }
    }
}
