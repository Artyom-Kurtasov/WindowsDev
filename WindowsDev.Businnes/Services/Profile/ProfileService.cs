using WindowsDev.Business.Primitives;
using WindowsDev.Business.Repositories.Interfaces;
using WindowsDev.Business.Services.PasswordManager;
using WindowsDev.Business.Services.PasswordManager.Hasher.Interfaces;
using WindowsDev.Business.Services.Profile.Interfaces;
using WindowsDev.Business.Services.UserManager.Interfaces;
using WindowsDev.Domain.DialogsMessages.Errors;

namespace WindowsDev.Business.Services.Profile
{
    public class ProfileService : IProfileService
    {
        private readonly ICurrentUserService _currentUserService;
        private readonly IUserRepository _userRepository;
        private readonly IHasherFactory _hasherFactory;
        private readonly IPasswordChanger _passwordChanger;

        public ProfileService(IUserRepository userRepository,
                              IHasherFactory hasherFactory,
                              ICurrentUserService currentUserService,
                              IPasswordChanger passwordChanger)
        {
            _userRepository = userRepository;
            _hasherFactory = hasherFactory;
            _currentUserService = currentUserService;
            _passwordChanger = passwordChanger;
        }

        public async Task<Result<int>> ChangePasswordAsync(string currentPassword, string newPassword, string confirmPassword)
        {
            var result = ValidateInputPassword(newPassword, currentPassword, confirmPassword);

            if (!result.IsSuccess)
                return Result<int>.Failure(result.Error);

            var user = await _userRepository.GetByLoginAsync(_currentUserService.Login);
            ArgumentNullException.ThrowIfNull(user);

            _passwordChanger.IsRecoveryMode = false;
            var changeResult = await _passwordChanger.ChangeUserPasswordAsync(_currentUserService.Login,
                newPassword, currentPassword);

            if (!changeResult.IsSuccess)
                return Result<int>.Failure(changeResult.Error);

            return Result<int>.Success(changeResult.Value);
        }

        public async Task<Result<bool>> ChangeUsernameAsync(string currentUsername, string newUsername)
        {
            var result = await ValidateInputUsernameAsync(currentUsername, newUsername);

            if (!result.IsSuccess)
                return result;

            var user = await _userRepository.GetByLoginAsync(_currentUserService.Login);
            ArgumentNullException.ThrowIfNull(user);

            _currentUserService.Username = newUsername;
            user.Username = newUsername;
            await _userRepository.UpdateAsync(user);

            return Result<bool>.Success(true);

        }

        private Result<bool> ValidateInputPassword(string newPassword, string currentPassword, string confirmPassword)
        {
            if (newPassword == currentPassword)
                return Result<bool>.Failure(ProfileErrors.NewPasswordSameAsCurrent);

            if (newPassword != confirmPassword)
                return Result<bool>.Failure(ProfileErrors.PasswordsDontMatch);

            return Result<bool>.Success(true);
        }

        private async Task<Result<bool>> ValidateInputUsernameAsync(string currentUsername, string newUsername)
        {
            if (currentUsername == newUsername)
                return Result<bool>.Failure(ProfileErrors.NewUsernameSameAsCurrent);

            if (await _userRepository.ExistsByUsernameAsync(newUsername))
                return Result<bool>.Failure(ProfileErrors.UsernamAlreadyTake);

            return Result<bool>.Success(true);
        }
    }
}
