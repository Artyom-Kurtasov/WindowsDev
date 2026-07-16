using Moq;
using WindowsDev.Business.Repositories.Interfaces;
using WindowsDev.Business.Services.PasswordManager;
using WindowsDev.Business.Services.PasswordManager.Hasher;
using WindowsDev.Business.Services.PasswordManager.PasswordRecovery.Interfaces;
using WindowsDev.Business.Services.Registration;
using WindowsDev.Business.Services.UserManager.Interfaces;
using WindowsDev.Domain.UsersModels;
using WindowsDev.Domain.UsersModels.Enums;
using RegistrationService = WindowsDev.Business.Services.Registration.Registration;

namespace WindowsDev.Tests.Business.Registration
{
    public class RegistrationTest
    {
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<ICurrentUserService> _currentUserServiceMock;
        private readonly DefaultHasher _passwordHasher;
        private readonly Mock<IPasswordRecoveryService> _passwordRecoveryServiceMock;
        private readonly Mock<IPasswordChanger> _passwordChangerMock;

        public RegistrationTest()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _currentUserServiceMock = new Mock<ICurrentUserService>();
            _passwordHasher = new DefaultHasher();
            _passwordRecoveryServiceMock = new Mock<IPasswordRecoveryService>();
            _passwordChangerMock = new Mock<IPasswordChanger>();
        }

        private RegistrationService CreateService()
        {
            return new RegistrationService(
                _userRepositoryMock.Object,
                _currentUserServiceMock.Object,
                _passwordHasher,
                _passwordRecoveryServiceMock.Object,
                _passwordChangerMock.Object);
        }

        [Fact]
        public async Task Register_WhenAllCorrect_ReturnsRecoveryCode()
        {
            var expectedRecoveryCode = 123456;

            _passwordChangerMock
                .Setup(x => x.GenerateRecoveryCode())
                .Returns(expectedRecoveryCode);

            _userRepositoryMock
                .Setup(x => x.AddAsync(It.IsAny<UsersInfo>()))
                .Returns(Task.CompletedTask);

            var registration = CreateService();

            var result = await registration.Register(
                "password",
                "login",
                "username");

            Assert.True(result.IsSuccess);
            Assert.Equal(expectedRecoveryCode, result.Value);

            _userRepositoryMock.Verify(
                x => x.AddAsync(It.Is<UsersInfo>(u =>
                    u.Login == "login" &&
                    u.Username == "username" &&
                    u.PasswordHash != null &&
                    u.Salt != null &&
                    u.RecoveryCodeHash != null &&
                    u.RecoveryCodeSalt != null)),
                Times.Once);

            _currentUserServiceMock.Verify(
                x => x.SetUser(
                    It.IsAny<int>(),
                    "login",
                    "username"),
                Times.Once);
        }

        [Fact]
        public async Task Register_WhenAllCorrect_GeneratesPasswordAndRecoveryCodeHashes()
        {
            var expectedRecoveryCode = 456789;

            _passwordChangerMock
                .Setup(x => x.GenerateRecoveryCode())
                .Returns(expectedRecoveryCode);

            var registration = CreateService();

            var result = await registration.Register(
                "password",
                "login",
                "username");

            Assert.True(result.IsSuccess);
            Assert.Equal(expectedRecoveryCode, result.Value);

            _userRepositoryMock.Verify(
                x => x.AddAsync(It.Is<UsersInfo>(u =>
                    !string.IsNullOrEmpty(u.PasswordHash) &&
                    u.Salt != null &&
                    !string.IsNullOrEmpty(u.RecoveryCodeHash) &&
                    u.RecoveryCodeSalt != null)),
                Times.Once);
        }

        [Fact]
        public async Task Register_WhenRepositoryThrows_PropagatesException()
        {
            _passwordChangerMock
                .Setup(x => x.GenerateRecoveryCode())
                .Returns(123456);

            _userRepositoryMock
                .Setup(x => x.AddAsync(It.IsAny<UsersInfo>()))
                .ThrowsAsync(new InvalidOperationException("Database error"));

            var registration = CreateService();

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                registration.Register(
                    "password",
                    "login",
                    "username"));

            _currentUserServiceMock.Verify(
                x => x.SetUser(
                    It.IsAny<int>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()),
                Times.Never);
        }

        [Fact]
        public async Task Register_WhenAllCorrect_SetsCurrentUser()
        {
            var userId = 1;

            _passwordChangerMock
                .Setup(x => x.GenerateRecoveryCode())
                .Returns(789012);

            _userRepositoryMock
                .Setup(x => x.AddAsync(It.IsAny<UsersInfo>()))
                .Callback<UsersInfo>(u => u.Id = userId);

            var registration = CreateService();

            var result = await registration.Register(
                "password",
                "login",
                "username");

            Assert.True(result.IsSuccess);

            _currentUserServiceMock.Verify(
                x => x.SetUser(
                    userId,
                    "login",
                    "username"),
                Times.Once);
        }

        [Fact]
        public async Task Register_WhenAllCorrect_UserHasDefaultHashMethod()
        {
            _passwordChangerMock
                .Setup(x => x.GenerateRecoveryCode())
                .Returns(111222);

            var registration = CreateService();

            var result = await registration.Register(
                "password",
                "login",
                "username");

            Assert.True(result.IsSuccess);

            _userRepositoryMock.Verify(
                x => x.AddAsync(It.Is<UsersInfo>(u =>
                    u.HashMethod == HashMethod.Default)),
                Times.Once);
        }
    }
}