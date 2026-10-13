namespace WindowsDev.Application.Identity.Cryptography;

public interface IHasherBase
{
    ulong HashValue(string password, byte[] salt);
    byte[] GenerateSalt();
}