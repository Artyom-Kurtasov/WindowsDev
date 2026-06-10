using Moq;
using WindowsDev.Business.Repositories.Interfaces;
using Service = WindowsDev.Business.Services.Registration;
using WindowsDev.Business.Services.UserManager.Interfaces;
using WindowsDev.Domain.UsersModels;
using WindowsDev.Business.Services.PasswordManager.Hasher;

namespace WindowsDev.Tests.Business.Registration
{
    public class RegistrationTest
    {
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<ICurrentUserService> _currentUserServiceMock;
        private readonly DefaultPasswordHasher _passwordHasher;

        public RegistrationTest()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _currentUserServiceMock = new Mock<ICurrentUserService>();
            _passwordHasher = new DefaultPasswordHasher();
        }

        private Service.Registration CreateService()
        {
            return new Service.Registration(
                _userRepositoryMock.Object,
                _currentUserServiceMock.Object,
                _passwordHasher);
        }

        [Fact]
        public async Task Register_WhenPasswordEmpty_ReturnsFalse()
        {
            var registration = CreateService();

            var result = await registration.Register("", "login", "username");

            Assert.False(result);

            _userRepositoryMock.Verify(x =>
                x.ExistsByLoginAsync(It.IsAny<string>()), Times.Never);

            _userRepositoryMock.Verify(x =>
                x.AddAsync(It.IsAny<UsersInfo>()), Times.Never);

            _currentUserServiceMock.Verify(x =>
                x.SetUser(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()),
                Times.Never);
        }

        [Fact]
        public async Task Register_WhenUserAlreadyExist_ReturnsFalse()
        {
            _userRepositoryMock
                .Setup(x => x.ExistsByLoginAsync("login"))
                .ReturnsAsync(true);

            var registration = CreateService();

            var result = await registration.Register("password", "login", "username");

            Assert.False(result);

            _userRepositoryMock.Verify(x =>
                x.AddAsync(It.IsAny<UsersInfo>()), Times.Never);

            _currentUserServiceMock.Verify(x =>
                x.SetUser(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()),
                Times.Never);
        }

        [Fact]
        public async Task Register_WhenAllCorrect_ReturnsTrue()
        {
            _userRepositoryMock
                .Setup(x => x.ExistsByLoginAsync("login"))
                .ReturnsAsync(false);

            var registration = CreateService();

            var result = await registration.Register("password", "login", "username");

            Assert.True(result);

            _userRepositoryMock.Verify(x =>
                x.ExistsByLoginAsync("login"), Times.Once);

            _userRepositoryMock.Verify(x =>
                x.AddAsync(It.IsAny<UsersInfo>()), Times.Once);

            _currentUserServiceMock.Verify(x =>
                x.SetUser(It.IsAny<int>(), "login", "username"), Times.Once);
        }
    }
}