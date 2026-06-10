using System.Security.Cryptography;
using System.Text;
using WindowsDev.Business.Services.PasswordManager.Hasher.Interfaces;

namespace WindowsDev.Business.Services.PasswordManager.Hasher
{
    public abstract class PasswordHasherBase : IPasswordHasherBase
    {
        public abstract ulong HashSeed { get; }
        public abstract ulong MixingConstant { get; }
        public abstract int Iterations { get; }

        public ulong HashPassword(string password, byte[] salt)
        {
            ulong hash = HashSeed;

            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
            byte[] combinedData = new byte[passwordBytes.Length + salt.Length];

            Buffer.BlockCopy(passwordBytes, 0, combinedData, 0, passwordBytes.Length);
            Buffer.BlockCopy(salt, 0, combinedData, passwordBytes.Length, salt.Length);

            for (int i = 0; i < Iterations; i++)
            {
                foreach (byte b in combinedData)
                {
                    hash ^= b;
                    hash *= MixingConstant;
                    hash = (hash << 13) | (hash >> 51);
                }
            }

            return hash;
        }

        public byte[] GenerateSalt()
        {
            byte[] _salt = new byte[16];

            using var _random = RandomNumberGenerator.Create();
            _random.GetBytes(_salt);

            return _salt;
        }
    }
}
