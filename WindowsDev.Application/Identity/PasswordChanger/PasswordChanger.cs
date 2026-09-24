using WindowsDev.Application.Primitives;
using WindowsDev.Application.Users;
using WindowsDev.Domain.Entities;
using WindowsDev.Domain.Messages.DialogsMessages.Errors;

namespace WindowsDev.Application.Identity.PasswordChanger;

internal class PasswordChanger : IPasswordChanger
{
    private readonly IUserRepository _userRepository;
    private readonly IHasherFactory _hasherFactory;
    private readonly IJwtService _jwtService;

    private const string HashHexFormat = "x16";
    private const int MinRecoveryCode = 100000;
    private const int MaxRecoveryCode = 999999;

    public bool IsRecoveryMode { get; set; }

    public PasswordChanger(
        IJwtService jwtService,
        IUserRepository userRepository,
        IHasherFactory hasherFactory
    )
    {
        _userRepository = userRepository;
        _hasherFactory = hasherFactory;
        _jwtService = jwtService;
    }

    public async Task<Result<(int, string)>> ChangeUserPasswordAsync(
        string login,
        string newPassword,
        string currentPassword = ""
    )
    {
        var user = await _userRepository.GetByLoginAsync(login);
        if (user is null)
            return Result<(int, string)>.Failure(CommonErrors.UserNotFound);

        var hasher = _hasherFactory.GetHashMethod(user.HashMethod);

        var recoveryCode = GenerateRecoveryCode();

        var updateResult = UpdateUserCredentials(
            user,
            hasher,
            recoveryCode,
            currentPassword,
            newPassword
        );

        if (updateResult.IsFailure)
            return Result<(int, string)>.Failure(updateResult.Error);

        await _userRepository.UpdateAsync(user);

        var jwtToken = _jwtService.GenerateToken(user.Id, user.Login, user.Username);

        return Result<(int, string)>.Success((recoveryCode, jwtToken));
    }

    public int GenerateRecoveryCode() => Random.Shared.Next(MinRecoveryCode, MaxRecoveryCode);

    private Result<bool> UpdateUserCredentials(
        UserInfo user,
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