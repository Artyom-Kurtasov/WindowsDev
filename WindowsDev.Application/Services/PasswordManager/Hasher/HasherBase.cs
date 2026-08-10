using System.Security.Cryptography;
using System.Text;
using WindowsDev.Application.Services.PasswordManager.Hasher.Interfaces;

namespace WindowsDev.Application.Services.PasswordManager.Hasher
{
    internal abstract class HasherBase : IHasherBase
    {
        private const int SaltSize = 16;
        private const int RotationBitsLeft = 13;
        private const int RotationBitsRight = 51;
        protected abstract ulong HashSeed { get; }
        protected abstract ulong MixingConstant { get; }
        protected abstract int Iterations { get; }

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
                    hash = (hash << RotationBitsLeft) | (hash >> RotationBitsRight);
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
