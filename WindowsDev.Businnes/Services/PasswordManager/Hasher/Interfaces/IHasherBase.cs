namespace WindowsDev.Business.Services.PasswordManager.Hasher.Interfaces
{
    public interface IHasherBase
    {
        ulong HashValue(string password, byte[] salt);
        byte[] GenerateSalt();
    }
}
