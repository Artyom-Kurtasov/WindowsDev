namespace WindowsDev.Business.Services.PasswordManager.Hasher.Interfaces
{
    public interface IHasherBase
    {
        ulong HashPassword(string password, byte[] salt);
        byte[] GenerateSalt();
    }
}
