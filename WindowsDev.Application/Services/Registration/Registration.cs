using WindowsDev.Application.Primitives;
using WindowsDev.Application.RepositoriesInterfaces;
using WindowsDev.Application.Services.PasswordManager;
using WindowsDev.Application.Services.PasswordManager.Hasher;
using WindowsDev.Application.Services.UserManager;
using WindowsDev.Domain.Entities;
using WindowsDev.Domain.Enums;

namespace WindowsDev.Application.Services.Registration
{
    public class Registration : IRegistration
    {
        private readonly IUserRepository _userRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly DefaultHasher _defaultHasher;
        private readonly IPasswordChanger _passwordChanger;

        private const string HashHexFormat = "x16";

        public Registration(
            IUserRepository userRepository,
            ICurrentUserService currentUserService,
            DefaultHasher defaultHasher,
            IPasswordChanger passwordChanger
        )
        {
            _userRepository = userRepository;
            _defaultHasher = defaultHasher;
            _currentUserService = currentUserService;
            _passwordChanger = passwordChanger;
        }

        public async Task<Result<int>> Register(string password, string login, string username)
        {
            var (passwordHash, passwordSalt) = HashPassword(password);

            var recoveryCode = _passwordChanger.GenerateRecoveryCode();
            var (recoveryCodeHash, recoveryCodeSalt) = HashRecoveryCode(recoveryCode);

            var user = new UsersInfo
            {
                Salt = passwordSalt,
                Username = username,
                Login = login,
                PasswordHash = passwordHash,
                HashMethod = (HashMethod)1,
                RecoveryCodeHash = recoveryCodeHash,
                RecoveryCodeSalt = recoveryCodeSalt,
            };

            await _userRepository.AddAsync(user);

            _currentUserService.SetUser(user.Id, user.Login, user.Username);

            return Result<int>.Success(recoveryCode);
        }

        public async Task<bool> IsLoginAvailableAsync(string login)
        {
            return !await _userRepository.ExistsByLoginAsync(login);
        }

        public async Task<bool> IsUsernameAvailableAsync(string username)
        {
            return !await _userRepository.ExistsByUsernameAsync(username);
        }

        private (string passwordHash, byte[] passwordSalt) HashPassword(string password)
        {
            var passwordSalt = _defaultHasher.GenerateSalt();
            var passwordHash = _defaultHasher
                .HashValue(password, passwordSalt)
                .ToString(HashHexFormat);

            return (passwordHash, passwordSalt);
        }

        private (string recoveryCodeHash, byte[] recoveryCodeSalt) HashRecoveryCode(
            int recoveryCode
        )
        {
            var recoveryCodeSalt = _defaultHasher.GenerateSalt();
            var recoveryCodeHash = _defaultHasher
                .HashValue(recoveryCode.ToString(), recoveryCodeSalt)
                .ToString(HashHexFormat);

            return (recoveryCodeHash, recoveryCodeSalt);
        }
    }
}
