using Moq;
using System.Text;
using WindowsDev.Business.Repositories.Interfaces;
using WindowsDev.Business.Services.Authorization;
using WindowsDev.Business.Services.PasswordManager;
using WindowsDev.Business.Services.PasswordManager.Hasher.Interfaces;
using WindowsDev.Business.Services.UserManager.Interfaces;
using WindowsDev.Domain.UsersModels;
using WindowsDev.Domain.UsersModels.Enums;

namespace WindowsDev.Tests.Business.Auth
{
    public class AuthorizationTest
    {
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<ICurrentUserService> _currentUserServiceMock;
        private readonly IPasswordHasherFactory _passwordHasherFactory;

        public AuthorizationTest()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _currentUserServiceMock = new Mock<ICurrentUserService>();

            var defaultHasher = new DefaultPasswordHasher();
            var simpleHasher = new SimplePasswordHasher();

            _passwordHasherFactory = new PasswordHasherFactory(defaultHasher, simpleHasher);
        }

        private Authorization CreateService()
        {
            return new Authorization(
                _userRepositoryMock.Object,
                _currentUserServiceMock.Object,
                _passwordHasherFactory);
        }

        [Theory]
        [InlineData("", "password")]
        [InlineData("login", "")]
        [InlineData(" ", "password")]
        public async Task Authorize_WhenInputEmpty_ReturnsFalse(string login, string password)
        {
            var auth = CreateService();

            var result = await auth.Authorize(login, password);

            Assert.False(result);

            _userRepositoryMock.Verify(x =>
                x.GetByLoginAsync(It.IsAny<string>()),
                Times.Never);

            _currentUserServiceMock.Verify(x =>
                x.SetUser(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()),
                Times.Never);
        }

        [Fact]
        public async Task Authorize_WhenUserNotFound_ReturnsFalse()
        {
            _userRepositoryMock
                .Setup(x => x.GetByLoginAsync("login"))
                .ReturnsAsync((UsersInfo?)null);

            var auth = CreateService();

            var result = await auth.Authorize("login", "password");

            Assert.False(result);

            _userRepositoryMock.Verify(x =>
                x.GetByLoginAsync("login"),
                Times.Once);

            _currentUserServiceMock.Verify(x =>
                x.SetUser(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()),
                Times.Never);
        }

        [Fact]
        public async Task Authorize_WhenPasswordIncorrect_ReturnsFalse()
        {
            var salt = Encoding.UTF8.GetBytes("salt");

            var hasher = _passwordHasherFactory.GetHashMethod(HashMethod.Default);
            var correctHash = hasher.HashPassword("correct", salt).ToString("x16");

            var user = new UsersInfo
            {
                Id = 1,
                Login = "admin",
                Username = "Artyom",
                Salt = salt,
                PasswordHash = correctHash,
                HashMethod = HashMethod.Default
            };

            _userRepositoryMock
                .Setup(x => x.GetByLoginAsync("admin"))
                .ReturnsAsync(user);

            var auth = CreateService();

            var result = await auth.Authorize("admin", "wrong");

            Assert.False(result);

            _currentUserServiceMock.Verify(x =>
                x.SetUser(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()),
                Times.Never);
        }

        [Fact]
        public async Task Authorize_WhenCorrect_ReturnsTrue()
        {
            var salt = Encoding.UTF8.GetBytes("salt");

            var hasher = _passwordHasherFactory.GetHashMethod(HashMethod.Default);
            var hash = hasher.HashPassword("password", salt).ToString("x16");

            var user = new UsersInfo
            {
                Id = 1,
                Login = "admin",
                Username = "Artyom",
                Salt = salt,
                PasswordHash = hash,
                HashMethod = HashMethod.Default
            };

            _userRepositoryMock
                .Setup(x => x.GetByLoginAsync("admin"))
                .ReturnsAsync(user);

            var auth = CreateService();

            var result = await auth.Authorize("admin", "password");

            Assert.True(result);

            _currentUserServiceMock.Verify(x =>
                x.SetUser(user.Id, user.Login, user.Username),
                Times.Once);
        }
    }
}