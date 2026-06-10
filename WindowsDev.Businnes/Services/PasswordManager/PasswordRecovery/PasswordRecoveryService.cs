using WindowsDev.Business.Repositories.Interfaces;
using WindowsDev.Business.Services.PasswordManager.Hasher.Interfaces;
using WindowsDev.Business.Services.UserManager.Interfaces;

namespace WindowsDev.Business.Services.PasswordManager.PasswordRecovery
{
    public class PasswordRecoveryService : IPasswordRecoveryService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasherFactory _passwordHasherFactory;

        public PasswordRecoveryService(IPasswordHasherFactory passwordHasherFactory,
                                       IUserRepository userRepository)
        {
            _passwordHasherFactory = passwordHasherFactory;
            _userRepository = userRepository;
        }

        public int GenerateRecoveryCode()
        {
            return Random.Shared.Next(100000, 999999);
        }

        public async Task<bool> IsRecoverCodeCorrect(int recoveryCode, string login)
        {
            var user = await _userRepository.GetByLoginAsync(login);

            if (user != null)
            {
                var hasher = _passwordHasherFactory.GetHashMethod(user.HashMethod);
                var recoveryCodeHash = hasher.HashPassword(recoveryCode.ToString(), user.RecoveryCodeSalt!);

                if (recoveryCodeHash.ToString("x16") == user.RecoveryCodeHash)
                    return true;
            }

            return false;
        }
    }
}
