using Moq;
using WindowsDev.Business.Repositories.Interfaces;
using WindowsDev.Business.Services.PasswordManager.Hasher;
using WindowsDev.Business.Services.PasswordManager.PasswordRecovery.Interfaces;
using WindowsDev.Business.Services.UserManager.Interfaces;
using WindowsDev.Domain.UsersModels;
using Service = WindowsDev.Business.Services.Registration;

namespace WindowsDev.Tests.Business.Registration
{
    public class RegistrationTest
    {
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<ICurrentUserService> _currentUserServiceMock;
        private readonly DefaultHasher _passwordHasher;
        private readonly Mock<IPasswordRecoveryService> _passwordRecoveryServiceMock;

        public RegistrationTest()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _currentUserServiceMock = new Mock<ICurrentUserService>();
            _passwordHasher = new DefaultHasher();
            _passwordRecoveryServiceMock = new Mock<IPasswordRecoveryService>();
        }

        private Service.Registration CreateService()
        {
            return new Service.Registration(
                _userRepositoryMock.Object,
                _currentUserServiceMock.Object,
                _passwordHasher,
                _passwordRecoveryServiceMock.Object);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("   ")]
        public async Task Register_WhenPasswordInvalid_ThrowsException(string password)
        {
            var registration = CreateService();

            await Assert.ThrowsAsync<Exception>(() =>
                registration.Register(password, "login", "username"));

            _userRepositoryMock.Verify(x =>
                x.ExistsByLoginAsync(It.IsAny<string>()), Times.Never);

            _userRepositoryMock.Verify(x =>
                x.AddAsync(It.IsAny<UsersInfo>()), Times.Never);

            _currentUserServiceMock.Verify(x =>
                x.SetUser(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()),
                Times.Never);
        }

        [Fact]
        public async Task Register_WhenUserAlreadyExist_ThrowsException()
        {
            _userRepositoryMock
                .Setup(x => x.ExistsByLoginAsync("login"))
                .ReturnsAsync(true);

            var registration = CreateService();

            await Assert.ThrowsAsync<Exception>(() =>
                registration.Register("password", "login", "username"));

            _userRepositoryMock.Verify(x =>
                x.AddAsync(It.IsAny<UsersInfo>()), Times.Never);

            _currentUserServiceMock.Verify(x =>
                x.SetUser(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()),
                Times.Never);
        }

        [Fact]
        public async Task Register_WhenAllCorrect_ReturnsRecoveryCode()
        {
            var expectedRecoveryCode = 123456;

            _userRepositoryMock
                .Setup(x => x.ExistsByLoginAsync("login"))
                .ReturnsAsync(false);

            _passwordRecoveryServiceMock
                .Setup(x => x.GenerateRecoveryCode())
                .Returns(expectedRecoveryCode);

            var registration = CreateService();

            var result = await registration.Register("password", "login", "username");

            Assert.Equal(expectedRecoveryCode, result);

            _userRepositoryMock.Verify(x =>
                x.ExistsByLoginAsync("login"), Times.Once);

            _userRepositoryMock.Verify(x =>
                x.AddAsync(It.Is<UsersInfo>(u =>
                    u.Login == "login" &&
                    u.Username == "username")), Times.Once);

            _currentUserServiceMock.Verify(x =>
                x.SetUser(It.IsAny<int>(), "login", "username"), Times.Once);
        }
    }
}