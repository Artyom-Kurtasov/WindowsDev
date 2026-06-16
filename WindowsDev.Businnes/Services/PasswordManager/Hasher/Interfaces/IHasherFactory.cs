using WindowsDev.Domain.UsersModels.Enums;

namespace WindowsDev.Business.Services.PasswordManager.Hasher.Interfaces
{
    public interface IHasherFactory
    {
        IHasherBase GetHashMethod(HashMethod method);
    }
}
