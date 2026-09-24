using WindowsDev.Application.Identity.PasswordChanger;
using WindowsDev.Application.Primitives;
using WindowsDev.Application.Users;
using WindowsDev.Domain.Entities;
using WindowsDev.Domain.Enums;
using WindowsDev.Domain.Messages.DialogsMessages.Warnings;

namespace WindowsDev.Application.Identity.Registration;

internal class Registration : IRegistration
{
    private readonly IUserRepository _userRepository;
    private readonly IDefaultHasher _defaultHasher;
    private readonly IPasswordChanger _passwordChanger;

    private const string HashHexFormat = "x16";

    public Registration(
        IUserRepository userRepository,
        IDefaultHasher defaultHasher,
        IPasswordChanger passwordChanger
    )
    {
        _userRepository = userRepository;
        _defaultHasher = defaultHasher;
        _passwordChanger = passwordChanger;
    }

    public async Task<Result<int>> Register(string password, string login, string username)
    {
        if (!await IsLoginAvailableAsync(login))
            return Result<int>.Failure(AuthDialogWarnings.LoginTaken);

        if (!await IsUsernameAvailableAsync(username))
            return Result<int>.Failure(AuthDialogWarnings.UsernameTaken);

        var (passwordHash, passwordSalt) = HashPassword(password);

        var recoveryCode = _passwordChanger.GenerateRecoveryCode();
        var (recoveryCodeHash, recoveryCodeSalt) = HashRecoveryCode(recoveryCode);

        var user = new UserInfo
        {
            Salt = passwordSalt,
            Username = username,
            Login = login,
            PasswordHash = passwordHash,
            HashMethod = HashMethod.Default,
            RecoveryCodeHash = recoveryCodeHash,
            RecoveryCodeSalt = recoveryCodeSalt,
        };

        await _userRepository.AddAsync(user);

        return Result<int>.Success(recoveryCode);
    }

    public async Task<bool> IsLoginAvailableAsync(string login) =>
        !await _userRepository.ExistsByLoginAsync(login);

    public async Task<bool> IsUsernameAvailableAsync(string username) =>
        !await _userRepository.ExistsByUsernameAsync(username);

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
