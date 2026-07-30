namespace WindowsDev.Application.Services.PasswordManager.Hasher.Interfaces
{
    public interface IHasherBase
    {
        ulong HashValue(string password, byte[] salt);
        byte[] GenerateSalt();
    }
}
