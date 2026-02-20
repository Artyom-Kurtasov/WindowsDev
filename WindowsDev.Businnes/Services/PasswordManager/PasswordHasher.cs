using System.CodeDom;
using System.Security.Cryptography;
using System.Text;

namespace WindowsDev.Businnes.Services.PasswordManager
{
    public class PasswordHasher
    {
        private const ulong _mixingConstant = 1415926535;
        private const ulong _hashSeed = 2687232455;

        public ulong HashPassword(string password, byte[] salt)
        {
            ulong hash = _hashSeed;
            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
            byte[] data = new byte[passwordBytes.Length + salt.Length];

            Buffer.BlockCopy(passwordBytes, 0, data, 0, passwordBytes.Length);
            Buffer.BlockCopy(salt, 0, data, passwordBytes.Length, salt.Length);

            for (int i = 0; i < 10000; i++)
            {
                foreach (byte b in data)
                {
                    hash ^= b;
                    hash *= _mixingConstant;
                    hash = (hash << 13) | (hash >> 51);
                }
            }

            return hash;
        }

        public byte[] GenerateSalt()
        {
            byte[] salt = new byte[16];

            using (var random = RandomNumberGenerator.Create())
            {
                random.GetBytes(salt);
            }

            return salt;
        }
    }
}
