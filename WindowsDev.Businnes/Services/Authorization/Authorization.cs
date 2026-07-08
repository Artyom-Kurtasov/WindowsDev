using WindowsDev.Business.Primitives;
using WindowsDev.Business.Repositories.Interfaces;
using WindowsDev.Business.Services.Authorization.Interfaces;
using WindowsDev.Business.Services.PasswordManager.Hasher.Interfaces;
using WindowsDev.Business.Services.UserManager.Interfaces;
using WindowsDev.Domain.DialogsMessages.Errors;

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

        public async Task<Result<bool>> Authorize(string login, string password)
        {
            if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
                return Result<bool>.Failure(AuthErrors.InvalidCredentials);

            var user = await _userRepository.GetByLoginAsync(login);

            if (user is null)
                return Result<bool>.Failure(AuthErrors.InvalidCredentials);

            var hasher = _hasherFactory.GetHashMethod(user.HashMethod);

            var hash = hasher.HashPassword(password, user.Salt);

            if (hash.ToString("x16") != user.PasswordHash)
                return Result<bool>.Failure(AuthErrors.InvalidCredentials);

            _currentUserService.SetUser(user.Id, user.Login, user.Username);

            return Result<bool>.Success(true);
        }
    }
}