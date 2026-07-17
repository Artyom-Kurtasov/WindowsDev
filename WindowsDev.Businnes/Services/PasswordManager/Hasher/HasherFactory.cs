using WindowsDev.Business.Services.PasswordManager.Hasher.Interfaces;
using WindowsDev.Domain.UsersModels.Enums;

namespace WindowsDev.Business.Services.PasswordManager.Hasher
{
    public class HasherFactory : IHasherFactory
    {
        private readonly DefaultHasher _defaultHasher;
        private readonly SimpleHasher _simplePasswordHasher;

        public HasherFactory(DefaultHasher defaultHasher, SimpleHasher simplePasswordHasher)
        {
            _defaultHasher = defaultHasher;
            _simplePasswordHasher = simplePasswordHasher;
        }

        public IHasherBase GetHashMethod(HashMethod method)
        {
            return method switch
            {
                HashMethod.Simple => _simplePasswordHasher,
                _ => _defaultHasher,
            };
        }
    }
}
