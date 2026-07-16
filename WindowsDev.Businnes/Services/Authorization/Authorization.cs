using WindowsDev.Business.Primitives;
using WindowsDev.Business.Repositories.Interfaces;
using WindowsDev.Business.Services.Authorization.Interfaces;
using WindowsDev.Business.Services.PasswordManager.Hasher.Interfaces;
using WindowsDev.Business.Services.UserManager.Interfaces;
using WindowsDev.Domain.DialogsMessages.Errors;
using WindowsDev.Domain.UsersModels;

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

            if (!VerifyPassword(password, user))
                return Result<bool>.Failure(AuthErrors.InvalidCredentials);

            _currentUserService.SetUser(user.Id, user.Login, user.Username);

            return Result<bool>.Success(true);
        }

        private bool VerifyPassword(string password, UsersInfo user)
        {
            const string HashHexFormat = "x16";

            var hasher = _hasherFactory.GetHashMethod(user.HashMethod);
            var hash = hasher.HashValue(password, user.Salt);

            return hash.ToString(HashHexFormat) == user.PasswordHash;
        }
    }
}