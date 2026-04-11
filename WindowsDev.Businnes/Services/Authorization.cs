using WindowsDev.Business.DataBase;
using WindowsDev.Business.Services.PasswordManager;
using WindowsDev.Business.Services.UserManager;
using WindowsDev.Domain.UsersAuthInfo;

namespace WindowsDev.Business.Services
{
    /// <summary>
    /// Handles user authorization by verifying login credentials.
    /// </summary>
    public class Authorization
    {
        private readonly DbManager _dbManager;
        private readonly PasswordHasher _passwordHasher;
        private CurrentUserService _currentUserService;

        private UsersInfo? _user;
        private byte[] _salt;
        private ulong _passwordHash;
        private string _passwordHashHex = string.Empty;

        public Authorization(
            PasswordHasher passwordHasher,
            CurrentUserService currentUserService,
            DbManager dbManager)
        {
            _passwordHasher = passwordHasher;
            _currentUserService = currentUserService;
            _dbManager = dbManager;
        }

        /// <summary>
        /// Attempts to authorize a user with the provided login and password.
        /// </summary>
        public bool Authorize(string login, string password)
        {
            using var dbContext = _dbManager.Create();
            {
                _user = dbContext.UsersInfo.FirstOrDefault(u => u.Login == login);
            }

            if (_user != null && CheckPasswordHash(password))
            {
                _currentUserService.SetUser(_user.Id, _user.Login, _user.Username);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Verifies that the provided password matches the stored hash.
        /// </summary>
        private bool CheckPasswordHash(string password)
        {
            _salt = _user?.Salt ?? Array.Empty<byte>();
            _passwordHash = _passwordHasher.HashPassword(password, _salt);
            _passwordHashHex = _passwordHash.ToString("x16");

            return _user != null && _passwordHashHex == _user.PasswordHash;
        }
    }
}

