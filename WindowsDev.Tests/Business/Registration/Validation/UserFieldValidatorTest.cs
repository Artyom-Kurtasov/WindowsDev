using WindowsDev.Business.Services.Registration.Validation;

namespace WindowsDev.Tests.Business.Registration.Validation
{
    public class UserFieldValidatorTest
    {
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("short")]
        public void IsValid_WhenWeakPassword_ReturnsFalse(string password)
        {
            Assert.False(PasswordValidator.IsValid(password));
        }

        [Theory]
        [InlineData("aaaaaaaaaaaa")]
        [InlineData("AAAAAAAAAAAA")]
        [InlineData("123456789012")]
        public void IsValid_WhenMissingRequirements_ReturnsFalse(string password)
        {
            Assert.False(PasswordValidator.IsValid(password));
        }

        [Fact]
        public void IsValid_WhenStrongPassword_ReturnsTrue()
        {
            var result = PasswordValidator.IsValid("Aa1234567890!");

            Assert.True(result);
        }
    }
}
