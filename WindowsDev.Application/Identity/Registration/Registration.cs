using WindowsDev.Application.Identity.Cryptography;
using WindowsDev.Application.Identity.PasswordChanger;
using WindowsDev.Application.Identity.Repositories;
using WindowsDev.Application.Primitives;
using WindowsDev.Application.Users;
using WindowsDev.Domain.Entities;
using WindowsDev.Domain.Enums;
using WindowsDev.Domain.Messages.DialogsMessages.Warnings;

namespace WindowsDev.Application.Identity.Registration;

internal class Registration : IRegistration
{
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IJwtService _jwtService;
    private readonly IUserRepository _userRepository;
    private readonly IDefaultHasher _defaultHasher;
    private readonly IPasswordChanger _passwordChanger;

    private const string HashHexFormat = "x16";

    public Registration(
        IUserRepository userRepository,
        IDefaultHasher defaultHasher,
        IPasswordChanger passwordChanger,
        IJwtService jwtService,
        IRefreshTokenRepository refreshTokenRepository
    )
    {
        _userRepository = userRepository;
        _defaultHasher = defaultHasher;
        _passwordChanger = passwordChanger;
        _jwtService = jwtService;
        _refreshTokenRepository = refreshTokenRepository;
    }

    public async Task<Result<(int, string, UserRefreshToken)>> Register(string password, string login, string username)
    {
        if (!await IsLoginAvailableAsync(login))
            return Result<(int, string, UserRefreshToken)>.Failure(AuthDialogWarnings.LoginTaken);

        if (!await IsUsernameAvailableAsync(username))
            return Result<(int, string, UserRefreshToken)>.Failure(AuthDialogWarnings.UsernameTaken);

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

        var JwtToken = _jwtService.GenerateToken(user.Id, login, username);
        var RefreshToken = _jwtService.GenerateRefreshToken(user.Id);
        await _refreshTokenRepository.AddAsync(RefreshToken);

        return Result<(int, string, UserRefreshToken)>.Success((recoveryCode, JwtToken, RefreshToken));
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
