namespace WindowsDev.Business.Services.PasswordManager.Hasher.Interfaces
{
    public interface IPasswordHasherBase
    {
        ulong HashPassword(string password, byte[] salt);
        byte[] GenerateSalt();
    }
}
