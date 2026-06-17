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

        public async Task Authorize(string login, string password)
        {
            if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
                throw new Exception("AuthError_InvalidCredentials");

            var user = await _userRepository.GetByLoginAsync(login);

            if (user is null)
                throw new Exception("AuthError_InvalidCredentials");

            // Pick the hasher that matches the user's stored hash method.
            // Users registered at different times may have different hash algorithms
            var hasher = _hasherFactory.GetHashMethod(user.HashMethod);

            // Combine password with the user's unique salt, then hash.
            // The salt was randomly generated at registration and stored alongside the user
            var hash = hasher.HashPassword(password, user.Salt);

            // PasswordHash is stored as a hex string.
            // Hash is a byte array — convert to hex for case-insensitive comparison
            if (hash.ToString("x16") != user.PasswordHash)
                throw new Exception("AuthError_InvalidCredentials");

            _currentUserService.SetUser(user.Id, user.Login, user.Username);
        }
    }
}