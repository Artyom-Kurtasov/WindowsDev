using WindowsDev.Businnes.DataBase;
using WindowsDev.Businnes.Services.PasswordManager;
using WindowsDev.Businnes.Services.UserManager;
using WindowsDev.Domain.UsersAuthInfo;

namespace WindowsDev.Businnes.Services.Registration
{
    public class Registration
    {
        private readonly AppDbContext _appDbContext;
        private readonly PasswordHasher _passwordHasher;
        private CurrentUserService _currentUserService;

        private byte[] _salt;
        private ulong _passwordHash;
        private UsersInfo? _usersInfo;

        public Registration(AppDbContext appDbContext, PasswordHasher passwordHasher, CurrentUserService currentUserService)
        {
            _appDbContext = appDbContext;
            _passwordHasher = passwordHasher;
            _currentUserService = currentUserService;
        }

        public bool Registrate(string password, string login, string username)
        {
            ConvertPasswordToHash(password);
            if (AddToDatabase(login, username))
            {
                _currentUserService.setUser(_usersInfo.Id, _usersInfo.Login, _usersInfo.Username);
                return true;
            }

            return false;
        }

        private void ConvertPasswordToHash(string password)
        {
            _salt = _passwordHasher.GenerateSalt();
            _passwordHash = _passwordHasher.HashPassword(password, _salt);
        }

        private bool AddToDatabase(string login, string username)
        {
            _usersInfo = _appDbContext.UsersInfo.FirstOrDefault(x => x.Login == login);

            if (_usersInfo == null)
            {
                _usersInfo = new UsersInfo
                {
                    Salt = _salt,
                    Username = username,
                    PasswordHash = _passwordHash.ToString("x16"),
                    Login = login
                };

                _appDbContext.Add(_usersInfo);
                _appDbContext.SaveChanges();

                return true;
            }

            return false;
        }
    }
}
