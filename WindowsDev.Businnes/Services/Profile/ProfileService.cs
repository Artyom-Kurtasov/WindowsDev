using WindowsDev.Business.Repositories.Interfaces;
using WindowsDev.Business.Services.PasswordManager.Hasher.Interfaces;
using WindowsDev.Business.Services.Profile.Interfaces;
using WindowsDev.Business.Services.UserManager;
using WindowsDev.Business.Services.UserManager.Interfaces;

namespace WindowsDev.Business.Services.Profile
{
    public class ProfileService : IProfileService
    {
        private readonly ICurrentUserService _currentUserService;
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasherFactory _hasherFactory;

        public ProfileService(IUserRepository userRepositor,
                              IPasswordHasherFactory passwordHasherFactory,
                              ICurrentUserService currentUserService)
        {
            _userRepository = userRepositor;
            _hasherFactory = passwordHasherFactory;
            _currentUserService = currentUserService;
        }

        public async Task ChangePassword(string currentPassword, string newPassword, string confirmPassword)
        {
            if (newPassword == currentPassword)
                throw new Exception("ProfileError_NewPasswordSameAsCurrent");

            if (newPassword != confirmPassword)
                throw new Exception("ProfileError_PasswordsDoNotMatch");

            var user = await _userRepository.GetByLoginAsync(_currentUserService.Login);

            var hasher = _hasherFactory.GetHashMethod(user.HashMethod);

            var currentHash = hasher.HashPassword(currentPassword, user.Salt);

            if (currentHash.ToString("x16") != user.PasswordHash)
                throw new Exception("ProfileError_CurrentPasswordIncorrect");

            var newSalt = hasher.GenerateSalt();
            var newHash = hasher.HashPassword(newPassword, newSalt);

            user.Salt = newSalt;
            user.PasswordHash = newHash.ToString("x16");

            await _userRepository.UpdateAsync(user);
        }

        public async Task ChangeUsername(string currentUsername, string newUsername)
        {
            if (currentUsername == newUsername)
                throw new Exception("ProfileError_NewUsernameSameAsCurrent");

            if (await _userRepository.ExistsByLoginAsync(_currentUserService.Login))
            {
                throw new Exception("ProfileError_UsernameExists");
            }
            else
            {
                _currentUserService.Username = newUsername;
            }
        }
    }
}
