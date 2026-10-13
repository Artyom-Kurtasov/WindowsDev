using WindowsDev.Domain.Enums;

namespace WindowsDev.Application.Identity.Cryptography;

public interface IHasherFactory
{
    IHasherBase GetHashMethod(HashMethod method);
}