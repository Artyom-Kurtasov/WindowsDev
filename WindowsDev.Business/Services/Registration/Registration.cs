using WindowsDev.Business.DataBase;
using WindowsDev.Business.Services.PasswordManager;
using WindowsDev.Business.Services.UserManager;
using WindowsDev.Domain.UsersAuthInfo;
using System.Linq;

namespace WindowsDev.Business.Services.Registration
{
    /// <summary>
    /// Handles user registration, password hashing, and storing users in the database.
    /// </summary>
    public class Registration
    {
        private readonly DbManager _dbManager;
        private readonly PasswordHasher _passwordHasher;
        private readonly CurrentUserService _currentUserService;

        private byte[] _salt;
        private ulong _passwordHash;
        private UsersInfo? _userInfo;

        public Registration(DbManager dbManager, PasswordHasher passwordHasher, CurrentUserService currentUserService)
        {
            _dbManager = dbManager;
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
            using var dbContext = _dbManager.Create();

            _userInfo = dbContext.UsersInfo.FirstOrDefault(x => x.Login == login);

            if (_userInfo == null)
            {
                _userInfo = new UsersInfo
                {
                    Salt = _salt,
                    Username = username,
                    PasswordHash = _passwordHash.ToString("x16"),
                    Login = login
                };

                dbContext.Add(_userInfo);
                dbContext.SaveChanges();

                return true;
            }

            return false;
        }
    }
}

