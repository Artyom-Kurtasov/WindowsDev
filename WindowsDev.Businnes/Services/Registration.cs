using WindowsDev.Businnes.DataBase;
using WindowsDev.Domain.UsersAuthInfo;
using WindowsDev.Businnes.Services.PasswordManager;

namespace WindowsDev.Businnes.Services
{
    public class Registration
    {
        private readonly AppDbContext _appDbContext;
        private readonly UserAuthInfo _userAuthInfo;
        private readonly PasswordHasher _passwordHasher;

        private byte[] _salt;
        private ulong _passwordHash;

        public Registration(AppDbContext appDbContext, UserAuthInfo userAuthInfo, PasswordHasher passwordHasher)
        {
            _appDbContext = appDbContext;
            _userAuthInfo = userAuthInfo;
            _passwordHasher = passwordHasher;
        }

        public void Registrate(string password, string login)
        {
            ConvertPasswordToHash(password);
            AddToDatabase(login);
        }

        private void ConvertPasswordToHash(string password)
        {
            _salt = _passwordHasher.GenerateSalt();
           _passwordHash = _passwordHasher.HashPassword(password, _salt);
        }

        private void AddToDatabase(string login)
        {
            _userAuthInfo.Salt = _salt;
            _userAuthInfo.PasswordHash = _passwordHash.ToString("x16");
            _userAuthInfo.Login = login;

            _appDbContext.Add(_userAuthInfo);
            _appDbContext.SaveChanges();
        }
    }
}
