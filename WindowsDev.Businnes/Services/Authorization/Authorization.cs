using WindowsDev.Business.Repositories.Interfaces;
using WindowsDev.Business.Services.Authorization.Interfaces;
using WindowsDev.Business.Services.PasswordManager.Hasher.Interfaces;
using WindowsDev.Business.Services.UserManager.Interfaces;

namespace WindowsDev.Business.Services.Authorization
{

    public class Authorization : IAuthorization
    {
        private readonly IUserRepository _userRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly IPasswordHasherFactory _hasherFactory;

        public Authorization(IUserRepository userRepository,
            ICurrentUserService currentUserService,
            IPasswordHasherFactory hasherFactory)
        {
            _userRepository = userRepository;
            _currentUserService = currentUserService;
            _hasherFactory = hasherFactory;
        }

        public async Task<bool> Authorize(string login, string password)
        {
            if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
                return false;

            var user = await _userRepository.GetByLoginAsync(login);

            if (user is null)
                return false;

            var hasher = _hasherFactory.GetHashMethod(user.HashMethod);

            var hash = hasher.HashPassword(password, user.Salt);

            // Convert hash to hex string.
            if (hash.ToString("x16") != user.PasswordHash)
                return false;

            _currentUserService.SetUser(user.Id, user.Login, user.Username);

            return true;
        }
    }
}

