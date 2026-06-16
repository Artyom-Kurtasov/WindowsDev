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
        private readonly IHasherFactory _hasherFactory;

        public Authorization(IUserRepository userRepository,
            ICurrentUserService currentUserService,
            IHasherFactory hasherFactory)
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

            // Pick the hasher that matches the user's stored hash method.
            // Users registered at different times may have different hash algorithms
            var hasher = _hasherFactory.GetHashMethod(user.HashMethod);

            // Combine password with the user's unique salt, then hash.
            // The salt was randomly generated at registration and stored alongside the user
            var hash = hasher.HashPassword(password, user.Salt);

            // PasswordHash is stored as a hex string.
            // Hash is a byte array — convert to hex for case-insensitive comparison
            if (hash.ToString("x16") != user.PasswordHash)
                return false;

            _currentUserService.SetUser(user.Id, user.Login, user.Username);

            return true;
        }
    }
}