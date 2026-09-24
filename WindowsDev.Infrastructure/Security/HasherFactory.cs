using WindowsDev.Application.Identity;
using WindowsDev.Domain.Enums;

namespace WindowsDev.Infrastructure.Security;

internal class HasherFactory : IHasherFactory
{
    private readonly IDefaultHasher _defaultHasher;
    private readonly ISimpleHasher _simplePasswordHasher;

    public HasherFactory(IDefaultHasher defaultHasher, ISimpleHasher simplePasswordHasher)
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