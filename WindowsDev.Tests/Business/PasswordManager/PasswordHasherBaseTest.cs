using System.Text;
using WindowsDev.Business.Services.PasswordManager.Hasher;

namespace WindowsDev.Tests.Business.PasswordManager
{
    public class PasswordHasherBaseTest
    {
        private readonly DefaultHasher _passwordHasher = new();

        [Theory]
        [InlineData("password123", "salt")]
        [InlineData("password", "saltsalt")]
        public void HashPassword_WhenInputIsSame_ReturnsSameHash(string testPassword, string testSalt)
        {
            byte[] salt = Encoding.UTF8.GetBytes(testSalt);

            ulong hash1 = _passwordHasher.HashValue(testPassword, salt);
            ulong hash2 = _passwordHasher.HashValue(testPassword, salt);

            Assert.Equal(hash1, hash2);
        }

        [Fact]
        public void HashPassword_WhenInputIsNotSame_ReturnsDifferentHash()
        {
            byte[] salt = Encoding.UTF8.GetBytes("salt");

            ulong hash1 = _passwordHasher.HashValue("password", salt);
            ulong hash2 = _passwordHasher.HashValue("password123", salt);

            Assert.NotEqual(hash1, hash2);
        }

        [Fact]
        public void HashPassword_WhenSaltIsDifferent_ReturnsDifferentHash()
        {
            byte[] salt1 = Encoding.UTF8.GetBytes("salt1");
            byte[] salt2 = Encoding.UTF8.GetBytes("salt2");

            ulong hash1 = _passwordHasher.HashValue("password", salt1);
            ulong hash2 = _passwordHasher.HashValue("password", salt2);

            Assert.NotEqual(hash1, hash2);
        }

        [Fact]
        public void GenerateSalt_WhenCalledTwice_ReturnsDifferentSalt()
        {
            byte[] salt1 = _passwordHasher.GenerateSalt();
            byte[] salt2 = _passwordHasher.GenerateSalt();

            Assert.NotEqual(salt1, salt2);
        }

        [Fact]
        public void GenerateSalt_WhenCalled_Returns16Bytes()
        {
            byte[] salt = _passwordHasher.GenerateSalt();

            Assert.Equal(16, salt.Length);
        }
    }
}