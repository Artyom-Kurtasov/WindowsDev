using System.Security.Cryptography;
using System.Text;
using WindowsDev.Business.Services.PasswordManager.Hasher.Interfaces;

namespace WindowsDev.Business.Services.PasswordManager.Hasher
{
    public abstract class HasherBase : IHasherBase
    {
        private const int SaltSize = 16;
        private const int RotationBits = 13;
        private const int ULongBits = 64;
        public abstract ulong HashSeed { get; }
        public abstract ulong MixingConstant { get; }
        public abstract int Iterations { get; }

        public ulong HashValue(string password, byte[] salt)
        {
            ulong hash = HashSeed;

            byte[] combinedData = CombinePasswordAndSalt(password, salt);

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
            byte[] salt = new byte[SaltSize];

            using var random = RandomNumberGenerator.Create();
            random.GetBytes(salt);

            return salt;
        }

        private byte[] CombinePasswordAndSalt(string password, byte[] salt)
        {
            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
            byte[] combinedData = new byte[passwordBytes.Length + salt.Length];

            Buffer.BlockCopy(passwordBytes, 0, combinedData, 0, passwordBytes.Length);
            Buffer.BlockCopy(salt, 0, combinedData, passwordBytes.Length, salt.Length);

            return combinedData;
        }
    }
}
