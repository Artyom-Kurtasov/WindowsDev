using WindowsDev.Application.Identity;
using WindowsDev.Application.Primitives;
using WindowsDev.Domain.Messages.DialogsMessages.Errors;
using WindowsDev.Application.Identity.PasswordChanger;

namespace WindowsDev.Application.Users;

internal class ProfileService : IProfileService
{
    private readonly IUserSession _userSession;
    private readonly IUserRepository _userRepository;
    private readonly IPasswordChanger _passwordChanger;
    private readonly IJwtService _jwtService;

    public ProfileService(
        IUserRepository userRepository,
        IUserSession userSession,
        IPasswordChanger passwordChanger,
        IJwtService jwtService
    )
    {
        _userRepository = userRepository;
        _userSession = userSession;
        _passwordChanger = passwordChanger;
        _jwtService = jwtService;
    }

    public async Task<Result<(int, string)>> ChangePasswordAsync(
        string currentPassword,
        string newPassword,
        string confirmPassword
    )
    {
        var result = ValidateInputPassword(newPassword, currentPassword, confirmPassword);

        if (!result.IsSuccess)
            return Result<(int, string)>.Failure(result.Error);

        var user = await _userRepository.ExistsByLoginAsync(_userSession.Login);

        if (!user)
            return Result<(int, string)>.Failure(CommonErrors.UserNotFound);

        _passwordChanger.IsRecoveryMode = false;
        var changeResult = await _passwordChanger.ChangeUserPasswordAsync(
            _userSession.Login,
            newPassword,
            currentPassword
        );

        if (!changeResult.IsSuccess)
            return Result<(int, string)>.Failure(changeResult.Error);

        return Result<(int, string)>.Success((changeResult.Value.Item1, changeResult.Value.Item2));
    }

    public async Task<Result<string>> ChangeUsernameAsync(
        string currentUsername,
        string newUsername
    )
    {
        var result = await ValidateInputUsernameAsync(currentUsername, newUsername);

        if (!result.IsSuccess)
            return Result<string>.Failure(result.Error);

        var user = await _userRepository.GetByLoginAsync(_userSession.Login);
        if (user is null)
            return Result<string>.Failure(CommonErrors.UserNotFound);

        user.Username = newUsername;
        await _userRepository.UpdateAsync(user);
        string jwtToken = _jwtService.GenerateToken(user.Id, user.Login, newUsername);

        return Result<string>.Success(jwtToken);
    }

    private Result<bool> ValidateInputPassword(
        string newPassword,
        string currentPassword,
        string confirmPassword
    )
    {
        if (newPassword == currentPassword)
            return Result<bool>.Failure(ProfileErrors.NewPasswordSameAsCurrent);

        if (newPassword != confirmPassword)
            return Result<bool>.Failure(ProfileErrors.PasswordsDontMatch);

        return Result<bool>.Success(true);
    }

    private async Task<Result<bool>> ValidateInputUsernameAsync(
        string currentUsername,
        string newUsername
    )
    {
        if (currentUsername == newUsername)
            return Result<bool>.Failure(ProfileErrors.NewUsernameSameAsCurrent);

        if (await _userRepository.ExistsByUsernameAsync(newUsername))
            return Result<bool>.Failure(ProfileErrors.UsernamAlreadyTaken);

        return Result<bool>.Success(true);
    }
}