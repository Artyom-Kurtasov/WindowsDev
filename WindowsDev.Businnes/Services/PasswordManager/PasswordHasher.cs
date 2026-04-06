using System.Security.Cryptography;
using System.Text;

namespace WindowsDev.Businnes.Services.PasswordManager
{
    /// <summary>
    /// Provides methods for hashing passwords and generating salts.
    /// </summary>
    public class PasswordHasher
    {
        private const ulong HASH_SEED = 2687232455;
        private const ulong MIXING_CONSTANT = 1415926535;

        /// <summary>
        /// Hashes a password with the given salt using a custom iterative algorithm.
        /// </summary>
        public ulong HashPassword(string password, byte[] salt)
        {
            ulong hash = HASH_SEED;

            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
            byte[] combinedData = new byte[passwordBytes.Length + salt.Length];

            Buffer.BlockCopy(passwordBytes, 0, combinedData, 0, passwordBytes.Length);
            Buffer.BlockCopy(salt, 0, combinedData, passwordBytes.Length, salt.Length);

            for (int i = 0; i < 10000; i++)
            {
                foreach (byte b in combinedData)
                {
                    hash ^= b;
                    hash *= MIXING_CONSTANT;
                    hash = (hash << 13) | (hash >> 51);
                }
            }

            return hash;
        }

        /// <summary>
        /// Generates a cryptographically secure 16-byte salt.
        /// </summary>
        /// <returns>Random salt as byte array.</returns>
        public byte[] GenerateSalt()
        {
            byte[] _salt = new byte[16];

            using var _random = RandomNumberGenerator.Create();
            _random.GetBytes(_salt);

            return _salt;
        }
    }
}