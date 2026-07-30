using WindowsDev.Domain.Enums;

namespace WindowsDev.Application.Services.PasswordManager.Hasher.Interfaces
{
    public interface IHasherFactory
    {
        IHasherBase GetHashMethod(HashMethod method);
    }
}
