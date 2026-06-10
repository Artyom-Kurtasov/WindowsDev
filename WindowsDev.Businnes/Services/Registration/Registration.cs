using WindowsDev.Business.Repositories.Interfaces;
using WindowsDev.Business.Services.PasswordManager.Hasher;
using WindowsDev.Business.Services.PasswordManager.PasswordRecovery;
using WindowsDev.Business.Services.Registration.Interfaces;
using WindowsDev.Business.Services.UserManager.Interfaces;
using WindowsDev.Domain.UsersModels;
using WindowsDev.Domain.UsersModels.Enums;

namespace WindowsDev.Business.Services.Registration
{

    public class Registration : IRegistration
    {
        private readonly IUserRepository _userRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly DefaultPasswordHasher _defaultPasswordHasher;
        private readonly IPasswordRecoveryService _passwordRecoveryService;

        public Registration(IUserRepository userRepository,
                            ICurrentUserService currentUserService,
                            DefaultPasswordHasher defaultPasswordHasher,
                            IPasswordRecoveryService passwordRecoveryService)
        {
            _passwordRecoveryService = passwordRecoveryService;
            _userRepository = userRepository;
            _defaultPasswordHasher = defaultPasswordHasher;
            _currentUserService = currentUserService;
        }

        public async Task<(bool, int)> Register(string password, string login, string username)
        {
            if (string.IsNullOrWhiteSpace(password))
                return (false, -1);

            if (await _userRepository.ExistsByLoginAsync(login))
                return (false, -1);

            var passwordSalt = _defaultPasswordHasher.GenerateSalt();
            var passwordHash = _defaultPasswordHasher.HashPassword(password, passwordSalt);

            var recoveryCodeSalt = _defaultPasswordHasher.GenerateSalt();
            var recoveryCode = _passwordRecoveryService.GenerateRecoveryCode();
            var recoveryCodeHash = _defaultPasswordHasher.HashPassword(recoveryCode.ToString(), recoveryCodeSalt);

            var user = new UsersInfo
            {
                Salt = passwordSalt,
                Username = username,
                Login = login,
                PasswordHash = passwordHash.ToString("x16"),
                HashMethod = (HashMethod)1,
                RecoveryCodeHash = recoveryCodeHash.ToString("x16"),
                RecoveryCodeSalt = recoveryCodeSalt
            };

            await _userRepository.AddAsync(user);

            _currentUserService.SetUser(user.Id, user.Login, user.Username);

            return (true, recoveryCode);
        }
    }
}

