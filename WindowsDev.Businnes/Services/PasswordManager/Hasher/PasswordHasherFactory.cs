using WindowsDev.Business.Services.PasswordManager.Hasher.Interfaces;
using WindowsDev.Domain.UsersModels.Enums;

namespace WindowsDev.Business.Services.PasswordManager.Hasher
{
    public class PasswordHasherFactory : IPasswordHasherFactory
    {
        private readonly DefaultPasswordHasher _defaultPasswordHasher;
        private readonly SimplePasswordHasher _simplePasswordHasher;

        public PasswordHasherFactory(DefaultPasswordHasher defaultPasswordHasher, SimplePasswordHasher simplePasswordHasher)
        {
            _defaultPasswordHasher = defaultPasswordHasher;
            _simplePasswordHasher = simplePasswordHasher;
        }

        public IPasswordHasherBase GetHashMethod(HashMethod method)
        {
            return method switch
            {
                HashMethod.Simple => _simplePasswordHasher,
                _ => _defaultPasswordHasher
            };
        }
    }
}
