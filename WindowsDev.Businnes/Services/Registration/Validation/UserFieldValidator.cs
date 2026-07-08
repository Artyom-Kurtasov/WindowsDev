using WindowsDev.Business.Repositories.Interfaces;

namespace WindowsDev.Business.Services.Registration.Validation
{
    public class UserFieldValidator
    {
        private readonly IUserRepository _userRepository;

        public UserFieldValidator(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<bool> IsLoginAvailableAsync(string login)
        {

            return !await _userRepository.ExistsByLoginAsync(login);

        }

        public async Task<bool> IsUsernameAvailableAsync(string username)
        {
            return !await _userRepository.ExistsByUsernameAsync(username);
        }
    }
}


