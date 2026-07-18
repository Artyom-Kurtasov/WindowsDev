using WindowsDev.Business.Primitives;
using WindowsDev.Business.Repositories.Interfaces;
using WindowsDev.Business.Services.PasswordManager.Hasher.Interfaces;
using WindowsDev.Business.Services.PasswordManager.Interfaces;
using WindowsDev.Business.Services.UserManager.Interfaces;
using WindowsDev.Domain.DialogsMessages.Errors;
using WindowsDev.Domain.UsersModels;

namespace WindowsDev.Business.Services.PasswordManager
{
    public class PasswordChanger : IPasswordChanger
    {
        private readonly ICurrentUserService _currentUser;
        private readonly IUserRepository _userRepository;
        private readonly IHasherFactory _hasherFactory;

        private const string HashHexFormat = "x16";
        private const int MinRecoveryCode = 100000;
        private const int MaxRecoveryCode = 999999;

        public bool IsRecoveryMode { get; set; }

        public PasswordChanger(
            ICurrentUserService currentUser,
            IUserRepository userRepository,
            IHasherFactory hasherFactory
        )
        {
            _currentUser = currentUser;
            _userRepository = userRepository;
            _hasherFactory = hasherFactory;
        }

        public async Task<Result<int>> ChangeUserPasswordAsync(
            string login,
            string newPassword,
            string currentPassword = ""
        )
        {
            var user = await _userRepository.GetByLoginAsync(login);

            ArgumentNullException.ThrowIfNull(user);

            var hasher = _hasherFactory.GetHashMethod(user.HashMethod);

            var recoveryCode = GenerateRecoveryCode();

            var updateResult = UpdateUserCredentials(
                user,
                hasher,
                recoveryCode,
                currentPassword,
                newPassword
            );

            if (!updateResult.IsSuccess)
                return Result<int>.Failure(updateResult.Error);

            await _userRepository.UpdateAsync(user);

            return Result<int>.Success(recoveryCode);
        }

        public int GenerateRecoveryCode() => Random.Shared.Next(MinRecoveryCode, MaxRecoveryCode);

        private Result<bool> UpdateUserCredentials(
            UsersInfo user,
            IHasherBase hasher,
            int recoveryCode,
            string currentPassword,
            string newPassword
        )
        {
            if (!IsRecoveryMode)
            {
                var currentHash = hasher.HashValue(currentPassword, user.Salt);

                if (currentHash.ToString(HashHexFormat) != user.PasswordHash)
                    return Result<bool>.Failure(ProfileErrors.InvalidCurrentPassword);
            }

            var (newPasswordHash, newPasswordSalt) = GeneratePasswordHash(hasher, newPassword);

            var (recoveryCodeHash, recoveryCodeSalt) = GenerateRecoveryCodeHash(
                hasher,
                recoveryCode
            );

            user.PasswordHash = newPasswordHash.ToString(HashHexFormat);
            user.Salt = newPasswordSalt;
            user.RecoveryCodeHash = recoveryCodeHash.ToString(HashHexFormat);
            user.RecoveryCodeSalt = recoveryCodeSalt;

            return Result<bool>.Success(true);
        }

        private (ulong hash, byte[] salt) GeneratePasswordHash(IHasherBase hasher, string password)
        {
            var salt = hasher.GenerateSalt();
            return (hasher.HashValue(password, salt), salt);
        }

        private (ulong hash, byte[] salt) GenerateRecoveryCodeHash(
            IHasherBase hasher,
            int recoveryCode
        )
        {
            var salt = hasher.GenerateSalt();
            return (hasher.HashValue(recoveryCode.ToString(), salt), salt);
        }
    }
}
