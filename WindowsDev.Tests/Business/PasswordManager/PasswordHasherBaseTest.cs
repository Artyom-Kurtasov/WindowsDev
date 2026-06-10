using System.Text;
using WindowsDev.Business.Services.PasswordManager;

namespace WindowsDev.Tests.Business.PasswordManager
{
    public class PasswordHasherBaseTest
    {
        [Theory]
        [InlineData("password123", "salt")]
        [InlineData("password", "saltsalt")]
        public void HashPassword_WhenInputIsSame_ReturnsSameHash(string testPassword, string testSalt)
        {
            var passwordHasher = new DefaultPasswordHasher();

            byte[] salt = Encoding.UTF8.GetBytes(testSalt);

            ulong hash1 = passwordHasher.HashPassword(testPassword, salt);
            ulong hash2 = passwordHasher.HashPassword(testPassword, salt);

            Assert.Equal(hash1, hash2);
        }

        [Fact]
        public void HashPassword_WhenInputIsNotSame_ReturnsDifferentHash()
        {
            var passwordHasher = new DefaultPasswordHasher();

            byte[] salt = Encoding.UTF8.GetBytes("salt");

            ulong hash1 = passwordHasher.HashPassword("password", salt);
            ulong hash2 = passwordHasher.HashPassword("password123", salt);

            Assert.NotEqual(hash1, hash2);
        }

        [Fact]
        public void HashPassword_WhenSaltIsDifferent_ReturnsDifferentHash()
        {
            var passwordHasher = new DefaultPasswordHasher();

            byte[] salt1 = Encoding.UTF8.GetBytes("salt1");
            byte[] salt2 = Encoding.UTF8.GetBytes("salt2");

            ulong hash1 = passwordHasher.HashPassword("password", salt1);
            ulong hash2 = passwordHasher.HashPassword("password", salt2);

            Assert.NotEqual(hash1, hash2);
        }

        [Fact]
        public void GenerateSalt_WhenCalledTwice_ReturnsDifferentSalt()
        {
            var hasher = new DefaultPasswordHasher();

            byte[] salt1 = hasher.GenerateSalt();
            byte[] salt2 = hasher.GenerateSalt();

            Assert.NotEqual(salt1, salt2);
        }

        [Fact]
        public void GenerateSalt_WhenCalled_Returns16Bytes()
        {
            var hasher = new DefaultPasswordHasher();

            byte[] salt = hasher.GenerateSalt();

            Assert.Equal(16, salt.Length);
        }
    }
}
