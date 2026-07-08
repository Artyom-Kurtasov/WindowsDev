using WindowsDev.Business.Primitives;
using WindowsDev.Business.Repositories.Interfaces;
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

        public ProfileService(IUserRepository userRepository,
                              IHasherFactory hasherFactory,
                              ICurrentUserService currentUserService)
        {
            _userRepository = userRepository;
            _hasherFactory = hasherFactory;
            _currentUserService = currentUserService;
        }

        public async Task<Result<bool>> ChangePasswordAsync(string currentPassword, string newPassword, string confirmPassword)
        {
            if (newPassword == currentPassword)
                return Result<bool>.Failure(ProfileErrors.NewPasswordSameAsCurrent);

            if (newPassword != confirmPassword)
                return Result<bool>.Failure(ProfileErrors.PasswordsDontMatch);

            var user = await _userRepository.GetByLoginAsync(_currentUserService.Login);

            var hasher = _hasherFactory.GetHashMethod(user.HashMethod);

            var currentHash = hasher.HashPassword(currentPassword, user.Salt);

            if (currentHash.ToString("x16") != user.PasswordHash)
                return Result<bool>.Failure(ProfileErrors.InvalidCurrentPassword);

            var newSalt = hasher.GenerateSalt();
            var newHash = hasher.HashPassword(newPassword, newSalt);

            user.Salt = newSalt;
            user.PasswordHash = newHash.ToString("x16");

            await _userRepository.UpdateAsync(user);

            return Result<bool>.Success(true);
        }

        public async Task<Result<bool>> ChangeUsernameAsync(string currentUsername, string newUsername)
        {
            if (currentUsername == newUsername)
                return Result<bool>.Failure(ProfileErrors.NewUsernameSameAsCurrent);

            if (await _userRepository.ExistsByUsernameAsync(newUsername))
                return Result<bool>.Failure(ProfileErrors.UsernamAlreadyTake);

            var user = await _userRepository.GetByLoginAsync(_currentUserService.Login);
            if (user is null)
                return Result<bool>.Failure(CommonErrors.UserNotFound);

            _currentUserService.Username = newUsername;
            user.Username = newUsername;
            await _userRepository.UpdateAsync(user);

            return Result<bool>.Success(true);
        }
    }
}
