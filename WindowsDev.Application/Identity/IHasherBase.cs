namespace WindowsDev.Application.Identity;

public interface IHasherBase
{
    ulong HashValue(string password, byte[] salt);
    byte[] GenerateSalt();
}