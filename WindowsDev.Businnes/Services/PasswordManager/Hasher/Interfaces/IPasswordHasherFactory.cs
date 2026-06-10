using WindowsDev.Domain.UsersModels.Enums;

namespace WindowsDev.Business.Services.PasswordManager.Hasher.Interfaces
{
    public interface IPasswordHasherFactory
    {
        IPasswordHasherBase GetHashMethod(HashMethod method);
    }
}
