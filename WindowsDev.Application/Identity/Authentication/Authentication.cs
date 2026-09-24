using WindowsDev.Application.Primitives;
using WindowsDev.Application.Users;
using WindowsDev.Domain.Entities;
using WindowsDev.Domain.Messages.DialogsMessages.Errors;

namespace WindowsDev.Application.Identity.Authentication;

internal class Authentication : IAuthentication
{
    private readonly IJwtService _jwtService;
    private readonly IUserRepository _userRepository;
    private readonly IHasherFactory _hasherFactory;

    public Authentication(
        IUserRepository userRepository,
        IHasherFactory hasherFactory,
        IJwtService jwtService
    )
    {
        _userRepository = userRepository;
        _hasherFactory = hasherFactory;
        _jwtService = jwtService;
    }

    public async Task<Result<string>> Authenticate(string login, string password)
    {
        if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
            return Result<string>.Failure(AuthErrors.InvalidCredentials);

        var user = await _userRepository.GetByLoginAsync(login);

        if (user is null)
            return Result<string>.Failure(AuthErrors.InvalidCredentials);

        if (!VerifyPassword(password, user))
            return Result<string>.Failure(AuthErrors.InvalidCredentials);

        var jwtToken = _jwtService.GenerateToken(user.Id, login, user.Username);

        return Result<string>.Success(jwtToken);
    }

    private bool VerifyPassword(string password, UserInfo user)
    {
        const string HashHexFormat = "x16";

        var hasher = _hasherFactory.GetHashMethod(user.HashMethod);
        var hash = hasher.HashValue(password, user.Salt);

        return hash.ToString(HashHexFormat) == user.PasswordHash;
    }

}
