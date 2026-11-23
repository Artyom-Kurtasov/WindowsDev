using WindowsDev.Application.Identity.Cryptography;
using WindowsDev.Application.Identity.Repositories;
using WindowsDev.Application.Primitives;
using WindowsDev.Application.Users;
using WindowsDev.Domain.Entities;
using WindowsDev.Domain.Messages.DialogsMessages.Errors;

namespace WindowsDev.Application.Identity.Authentication;

internal class Authentication : IAuthentication
{
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IJwtService _jwtService;
    private readonly IUserRepository _userRepository;
    private readonly IHasherFactory _hasherFactory;

    public Authentication(
        IRefreshTokenRepository refreshTokenRepository,
        IUserRepository userRepository,
        IHasherFactory hasherFactory,
        IJwtService jwtService
    )
    {
        _userRepository = userRepository;
        _hasherFactory = hasherFactory;
        _jwtService = jwtService;
        _refreshTokenRepository = refreshTokenRepository;
    }

    public async Task<Result<(string, UserRefreshToken)>> Authenticate(string login, string password)
    {
        if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
            return Result<(string, UserRefreshToken)>.Failure(AuthErrors.InvalidCredentials);

        var user = await _userRepository.GetByLoginAsync(login);

        if (user is null)
            return Result<(string, UserRefreshToken)>.Failure(AuthErrors.InvalidCredentials);

        if (!VerifyPassword(password, user))
            return Result<(string, UserRefreshToken)>.Failure(AuthErrors.InvalidCredentials);

        var jwtToken = _jwtService.GenerateToken(user.Id, login, user.Username);
        var refreshToken = _jwtService.GenerateRefreshToken(user.Id);

        await _refreshTokenRepository.AddAsync(refreshToken);

        return Result<(string, UserRefreshToken)>.Success((jwtToken, refreshToken));
    }

    public async Task<Result<(string, UserRefreshToken)>> RefreshAsync(UserRefreshToken refreshToken)
    {
        var storedToken = await _refreshTokenRepository.GetByTokenAsync(refreshToken.Token);

        if (storedToken is null || storedToken.IsRevoked || storedToken.ExpiresAt <= DateTime.UtcNow)
            return Result<(string, UserRefreshToken)>.Failure(AuthErrors.InvalidRefreshToken);

        var user = await _userRepository.GetByIdAsync(storedToken.UserId);

        if (user is null)
            return Result<(string, UserRefreshToken)>.Failure(AuthErrors.InvalidRefreshToken);

        var jwtToken = _jwtService.GenerateToken(user.Id, user.Login, user.Username);
        var newRefreshToken = _jwtService.GenerateRefreshToken(user.Id);

        await _refreshTokenRepository.DeleteAsync(storedToken.Id);
        await _refreshTokenRepository.AddAsync(newRefreshToken);

        return Result<(string, UserRefreshToken)>.Success((jwtToken, newRefreshToken));
    }

    private bool VerifyPassword(string password, UserInfo user)
    {
        const string HashHexFormat = "x16";

        var hasher = _hasherFactory.GetHashMethod(user.HashMethod);
        var hash = hasher.HashValue(password, user.Salt);

        return hash.ToString(HashHexFormat) == user.PasswordHash;
    }

}
