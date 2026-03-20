using WindowsDev.Businnes.DataBase;
using WindowsDev.Businnes.Services.PasswordManager;
using WindowsDev.Domain.UsersAuthInfo;

namespace WindowsDev.Businnes.Services
{
    public class Authorization
    {
        private readonly PasswordHasher _passwordHasher;
        private readonly AppDbContext _appDbContext;

        private byte[] _salt;
        private ulong _passwordHash;
        private string _passwordHashHex;
        private UserAuthInfo? _user;

        public Authorization(PasswordHasher passwordHasher, AppDbContext appDbContext)
        {
            _passwordHasher = passwordHasher;
            _appDbContext = appDbContext;
        }

        public bool Authorize(string username, string password)
        {
            if (SearchUserInDatabase(username))
            {
                ConvertPasswordToHash(username, password);

                if (CheckPasswordHash())
                {
                    return true;
                }
            }
            return false;
        }
        private void ConvertPasswordToHash(string username, string password)
        {
                _user = _appDbContext.UserAuthInfo.FirstOrDefault(x => x.Login == username);

                _salt = _user.Salt;

                _passwordHash = _passwordHasher.HashPassword(password, _salt);
                _passwordHashHex = _passwordHash.ToString("x16");
        }
        private bool CheckPasswordHash()
        {
            if (_passwordHashHex == _user.PasswordHash)
            {
                return true;
            }
            return false;
        }
        private bool SearchUserInDatabase(string username)
        {
            if (_appDbContext.UserAuthInfo.Any(x => x.Login == username))
            {
                return true;
            }
            return false;
        }
    }
}
