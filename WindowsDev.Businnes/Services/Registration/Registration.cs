using WindowsDev.Business.Primitives;
using WindowsDev.Business.Repositories.Interfaces;
using WindowsDev.Business.Services.PasswordManager.Hasher;
using WindowsDev.Business.Services.PasswordManager.PasswordRecovery.Interfaces;
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
        private readonly DefaultHasher _defaultHasher;
        private readonly IPasswordRecoveryService _passwordRecoveryService;

        public Registration(IUserRepository userRepository,
                            ICurrentUserService currentUserService,
                            DefaultHasher defaultHasher,
                            IPasswordRecoveryService passwordRecoveryService)
        {
            _passwordRecoveryService = passwordRecoveryService;
            _userRepository = userRepository;
            _defaultHasher = defaultHasher;
            _currentUserService = currentUserService;
        }

        public async Task<Result<int>> Register(string password, string login, string username)
        {
            var passwordSalt = _defaultHasher.GenerateSalt();
            var passwordHash = _defaultHasher.HashPassword(password, passwordSalt);

            var recoveryCodeSalt = _defaultHasher.GenerateSalt();
            var recoveryCode = _passwordRecoveryService.GenerateRecoveryCode();
            var recoveryCodeHash = _defaultHasher.HashPassword(recoveryCode.ToString(), recoveryCodeSalt);

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

            return Result<int>.Success(recoveryCode);
        }
    }
}

