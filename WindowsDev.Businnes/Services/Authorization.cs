using WindowsDev.Businnes.DataBase;
using WindowsDev.Businnes.Services.PasswordManager;
using WindowsDev.Businnes.Services.UserManager;
using WindowsDev.Domain.UsersAuthInfo;

namespace WindowsDev.Businnes.Services
{
    public class Authorization
    {
        private readonly PasswordHasher _passwordHasher;
        private readonly AppDbContext _appDbContext;

        private CurrentUserService _currentUserService;
        private UsersInfo? _user;

        private byte[] _salt;
        private ulong _passwordHash;
        private string _passwordHashHex;

        public Authorization(PasswordHasher passwordHasher, AppDbContext appDbContext, CurrentUserService currentUserService)
        {
            _passwordHasher = passwordHasher;
            _appDbContext = appDbContext;
            _currentUserService = currentUserService;
        }

        public bool Authorize(string login, string password)
        {
            _user = _appDbContext.UsersInfo.FirstOrDefault(x => x.Login == login);

            if (_user != null)
            {
                if (CheckPasswordHash(password))
                {
                    _currentUserService.setUser(_user.Id, _user.Login, _user.Username);
                    return true;
                }
            }
            return false;
        }

        private bool CheckPasswordHash(string password)
        {
            _salt = _user.Salt;
            _passwordHash = _passwordHasher.HashPassword(password, _salt);
            _passwordHashHex = _passwordHash.ToString("x16");

            return _passwordHashHex == _user.PasswordHash;
        }
    }
}
