using WindowsDev.Business.Primitives;
using WindowsDev.Business.Repositories.Interfaces;
using WindowsDev.Business.Services.PasswordManager.Hasher.Interfaces;
using WindowsDev.Business.Services.PasswordManager.PasswordRecovery.Interfaces;
using WindowsDev.Domain.DialogsMessages.Errors;

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

        public async Task<Result<bool>> IsRecoverCodeCorrect(int recoveryCode, string login)
        {
            var user = await _userRepository.GetByLoginAsync(login);

            ArgumentNullException.ThrowIfNull(user);

            var hasher = _hasherFactory.GetHashMethod(user.HashMethod);
            var recoveryCodeHash = hasher.HashPassword(recoveryCode.ToString(), user.RecoveryCodeSalt!);

            if (recoveryCodeHash.ToString("x16") != user.RecoveryCodeHash)
                return Result<bool>.Failure(PasswordRecoveryErrors.InvalidRecoveryCode);

            return Result<bool>.Success(true);
        }

        public async Task<Result<int>> ChangePasswordAsync(string login, string password)
        {
            var user = await _userRepository.GetByLoginAsync(login);

            ArgumentNullException.ThrowIfNull(user);

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

            return Result<int>.Success(recoveryCode);
        }
    }
}
