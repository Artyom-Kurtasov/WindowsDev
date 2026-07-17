using System.Text;
using Moq;
using WindowsDev.Business.Repositories.Interfaces;
using WindowsDev.Business.Services.Authorization;
using WindowsDev.Business.Services.PasswordManager.Hasher;
using WindowsDev.Business.Services.PasswordManager.Hasher.Interfaces;
using WindowsDev.Business.Services.UserManager.Interfaces;
using WindowsDev.Domain.DialogsMessages.Errors;
using WindowsDev.Domain.UsersModels;
using WindowsDev.Domain.UsersModels.Enums;

namespace WindowsDev.Tests.Business.Auth
{
    public class AuthorizationTest
    {
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<ICurrentUserService> _currentUserServiceMock;
        private readonly IHasherFactory _hasherFactory;

        public AuthorizationTest()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _currentUserServiceMock = new Mock<ICurrentUserService>();

            var defaultHasher = new DefaultHasher();
            var simpleHasher = new SimpleHasher();

            _hasherFactory = new HasherFactory(defaultHasher, simpleHasher);
        }

        private Authorization CreateService()
        {
            return new Authorization(
                _userRepositoryMock.Object,
                _currentUserServiceMock.Object,
                _hasherFactory
            );
        }

        [Theory]
        [InlineData("", "password")]
        [InlineData("login", "")]
        [InlineData(" ", "password")]
        [InlineData("login", " ")]
        public async Task Authorize_WhenInputEmpty_ReturnsFailure(string login, string password)
        {
            var auth = CreateService();

            var result = await auth.Authorize(login, password);

            Assert.False(result.IsSuccess);
            Assert.Equal(AuthErrors.InvalidCredentials, result.Error);

            _userRepositoryMock.Verify(x => x.GetByLoginAsync(It.IsAny<string>()), Times.Never);

            _currentUserServiceMock.Verify(
                x => x.SetUser(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()),
                Times.Never
            );
        }

        [Fact]
        public async Task Authorize_WhenUserNotFound_ReturnsFailure()
        {
            _userRepositoryMock
                .Setup(x => x.GetByLoginAsync("login"))
                .ReturnsAsync((UsersInfo?)null);

            var auth = CreateService();

            var result = await auth.Authorize("login", "password");

            Assert.False(result.IsSuccess);
            Assert.Equal(AuthErrors.InvalidCredentials, result.Error);

            _userRepositoryMock.Verify(x => x.GetByLoginAsync("login"), Times.Once);

            _currentUserServiceMock.Verify(
                x => x.SetUser(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()),
                Times.Never
            );
        }

        [Fact]
        public async Task Authorize_WhenPasswordIncorrect_ReturnsFailure()
        {
            var salt = Encoding.UTF8.GetBytes("salt");

            var hasher = _hasherFactory.GetHashMethod(HashMethod.Default);
            var correctHash = hasher.HashValue("correct", salt).ToString("x16");

            var user = new UsersInfo
            {
                Id = 1,
                Login = "admin",
                Username = "Artyom",
                Salt = salt,
                PasswordHash = correctHash,
                HashMethod = HashMethod.Default,
            };

            _userRepositoryMock.Setup(x => x.GetByLoginAsync("admin")).ReturnsAsync(user);

            var auth = CreateService();

            var result = await auth.Authorize("admin", "wrong");

            Assert.False(result.IsSuccess);
            Assert.Equal(AuthErrors.InvalidCredentials, result.Error);

            _currentUserServiceMock.Verify(
                x => x.SetUser(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()),
                Times.Never
            );
        }

        [Fact]
        public async Task Authorize_WhenCorrect_CompletesSuccessfully()
        {
            var salt = Encoding.UTF8.GetBytes("salt");

            var hasher = _hasherFactory.GetHashMethod(HashMethod.Default);
            var hash = hasher.HashValue("password", salt).ToString("x16");

            var user = new UsersInfo
            {
                Id = 1,
                Login = "admin",
                Username = "Artyom",
                Salt = salt,
                PasswordHash = hash,
                HashMethod = HashMethod.Default,
            };

            _userRepositoryMock.Setup(x => x.GetByLoginAsync("admin")).ReturnsAsync(user);

            var auth = CreateService();

            var result = await auth.Authorize("admin", "password");

            Assert.True(result.IsSuccess);
            Assert.True(result.Value);

            _currentUserServiceMock.Verify(
                x => x.SetUser(user.Id, user.Login, user.Username),
                Times.Once
            );
        }
    }
}
