using WindowsDev.Business.Repositories.Interfaces;
using WindowsDev.Business.Services.PasswordManager.Hasher.Interfaces;

namespace WindowsDev.Business.Services.PasswordManager.PasswordRecovery
{
    public class PasswordRecoveryService : IPasswordRecoveryService
    {
        private readonly IUserRepository _userRepository;
        private readonly IHasherFactory _hasherFactory;

        public PasswordRecoveryService(IHasherFactory hasherFactory,
                                       IUserRepository userRepository)
        {
            _hasherFactory = hasherFactory;
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
                var hasher = _hasherFactory.GetHashMethod(user.HashMethod);
                var recoveryCodeHash = hasher.HashPassword(recoveryCode.ToString(), user.RecoveryCodeSalt!);

                if (recoveryCodeHash.ToString("x16") == user.RecoveryCodeHash)
                    return true;
            }

            return false;
        }

        public async Task<int> ChangePasswordAsync(string login, string password)
        {
            var user = await _userRepository.GetByLoginAsync(login);

            if (user != null)
            {
                var hasher = _hasherFactory.GetHashMethod(user.HashMethod);
                var passwordSalt = hasher.GenerateSalt();
                var recoveryCodeSalt = hasher.GenerateSalt();

                var recoveryCode = GenerateRecoveryCode();
                var recoveryCodeHash = hasher.HashPassword(recoveryCode.ToString(), recoveryCodeSalt);
                var passwordHash = hasher.HashPassword(password, passwordSalt);

                user.RecoveryCodeHash = recoveryCodeHash.ToString("x16");
                user.PasswordHash = passwordHash.ToString("x16");
                user.Salt = passwordSalt;
                user.RecoveryCodeSalt = recoveryCodeSalt;


                await _userRepository.UpdateAsync(user);

                return recoveryCode;
            }

            return -1;
        }
    }
}
