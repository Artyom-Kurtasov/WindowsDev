using WindowsDev.Application.Identity.PasswordChanger;
using WindowsDev.Application.Primitives;
using WindowsDev.Application.Users;
using WindowsDev.Domain.Messages.DialogsMessages.Errors;

namespace WindowsDev.Application.Identity.PasswordRecovery;

public class PasswordRecoveryService : IPasswordRecoveryService
{
    private readonly IUserRepository _userRepository;
    private readonly IHasherFactory _hasherFactory;
    private readonly IPasswordChanger _passwordChanger;

    private const string HashHexFormat = "x16";

    public PasswordRecoveryService(
        IHasherFactory hasherFactory,
        IUserRepository userRepository,
        IPasswordChanger passwordChanger
    )
    {
        _hasherFactory = hasherFactory;
        _userRepository = userRepository;
        _passwordChanger = passwordChanger;
    }

    public async Task<Result<bool>> IsRecoverCodeCorrectAsync(int recoveryCode, string login)
    {
        var user = await _userRepository.GetByLoginAsync(login);

        if (user is null)
        {
            return Result<bool>.Failure(PasswordRecoveryErrors.InvalidRecoveryCode);
        }

        var hasher = _hasherFactory.GetHashMethod(user.HashMethod);
        var recoveryCodeHash = hasher.HashValue(
            recoveryCode.ToString(),
            user.RecoveryCodeSalt!
        );

        return recoveryCodeHash.ToString(HashHexFormat) == user.RecoveryCodeHash
            ? Result<bool>.Success(true)
            : Result<bool>.Failure(PasswordRecoveryErrors.InvalidRecoveryCode);
    }

    public async Task<Result<int>> ChangePasswordAsync(string login, string password)
    {
        _passwordChanger.IsRecoveryMode = true;
        return await _passwordChanger.ChangeUserPasswordAsync(login, password);
    }

    public async Task<Result<bool>> IsUserExistAsync(string login)
    {
        var isExist = await _userRepository.ExistsByLoginAsync(login);
        return Result<bool>.Success(isExist);
    }
}