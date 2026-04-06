using WindowsDev.Businnes.DataBase;
using WindowsDev.Businnes.Services.PasswordManager;
using WindowsDev.Businnes.Services.UserManager;
using WindowsDev.Domain.UsersAuthInfo;
using System.Linq;

namespace WindowsDev.Businnes.Services.Registration
{
    /// <summary>
    /// Handles user registration, password hashing, and storing users in the database.
    /// </summary>
    public class Registration
    {
        private readonly AppDbContext _appDbContext;
        private readonly PasswordHasher _passwordHasher;
        private readonly CurrentUserService _currentUserService;

        private byte[] _salt;
        private ulong _passwordHash;
        private UsersInfo? _userInfo;

        public Registration(AppDbContext appDbContext, PasswordHasher passwordHasher, CurrentUserService currentUserService)
        {
            _appDbContext = appDbContext;
            _passwordHasher = passwordHasher;
            _currentUserService = currentUserService;
        }

        /// <summary>
        /// Registers a new user with the provided login, username, and password.
        /// </summary>
        public bool Registrate(string password, string login, string username)
        {
            GeneratePasswordHash(password);

            if (AddUserToDatabase(login, username))
            {
                _currentUserService.SetUser(_userInfo.Id, _userInfo.Login, _userInfo.Username);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Generates a salt and password hash for secure storage.
        /// </summary>
        private void GeneratePasswordHash(string password)
        {
            _salt = _passwordHasher.GenerateSalt();
            _passwordHash = _passwordHasher.HashPassword(password, _salt);
        }

        /// <summary>
        /// Adds a new user to the database if the login is not already taken.
        /// </summary>
        private bool AddUserToDatabase(string login, string username)
        {
            _userInfo = _appDbContext.UsersInfo.FirstOrDefault(x => x.Login == login);

            if (_userInfo == null)
            {
                _userInfo = new UsersInfo
                {
                    Salt = _salt,
                    Username = username,
                    PasswordHash = _passwordHash.ToString("x16"),
                    Login = login
                };

                _appDbContext.Add(_userInfo);
                _appDbContext.SaveChanges();

                return true;
            }

            return false;
        }
    }
}