using System.Text;
using Konscious.Security.Cryptography;
using System.Security.Cryptography;

namespace WindowsDev.Businnes.Services.PasswordManager
{
    public class PasswordHasher
    {
        private const int _degreeOfParallelism = 4;
        private const int _memorySize = 65536;
        private const int _iterations = 4;
        private const int _saltSize = 16;

        public byte[] HashPassword(string password, byte[] salt)
        {
            using (var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password)))
            {
                argon2.Iterations = _iterations;
                argon2.Salt = salt;
                argon2.MemorySize = _memorySize;
                argon2.DegreeOfParallelism = _degreeOfParallelism;

                return argon2.GetBytes(32);
            }
        }

        public byte[] GenerateSalt()
        {
            byte[] salt = new byte[_saltSize];

            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }
            return salt;
        }
    }
}
