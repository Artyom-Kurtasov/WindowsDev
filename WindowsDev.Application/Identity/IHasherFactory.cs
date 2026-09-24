using WindowsDev.Domain.Enums;

namespace WindowsDev.Application.Identity;

public interface IHasherFactory
{
    IHasherBase GetHashMethod(HashMethod method);
}