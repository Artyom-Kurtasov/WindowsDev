using Moq;
using WindowsDev.Business.Primitives;
using WindowsDev.Business.Repositories.Interfaces;
using WindowsDev.Business.Services.PasswordManager;
using WindowsDev.Business.Services.PasswordManager.Hasher.Interfaces;
using WindowsDev.Business.Services.PasswordManager.PasswordRecovery;
using WindowsDev.Domain.DialogsMessages.Errors;
using WindowsDev.Domain.UsersModels;
using WindowsDev.Domain.UsersModels.Enums;

namespace WindowsDev.Tests.Business.PasswordManager
{
    public class PasswordRecoveryServiceTest
    {
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<IHasherFactory> _hasherFactoryMock;
        private readonly Mock<IPasswordChanger> _passwordChangerMock;

        public PasswordRecoveryServiceTest()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _hasherFactoryMock = new Mock<IHasherFactory>();
            _passwordChangerMock = new Mock<IPasswordChanger>();
        }

        private PasswordRecoveryService CreateService()
        {
            return new PasswordRecoveryService(
                _hasherFactoryMock.Object,
                _userRepositoryMock.Object,
                _passwordChangerMock.Object
            );
        }

        [Fact]
        public async Task IsRecoverCodeCorrectAsync_WhenUserNotFound_ThrowsException()
        {
            var login = "unknown";

            _userRepositoryMock.Setup(x => x.GetByLoginAsync(login)).ReturnsAsync((UsersInfo?)null);

            var service = CreateService();

            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                service.IsRecoverCodeCorrectAsync(123456, login)
            );

            _userRepositoryMock.Verify(x => x.GetByLoginAsync(login), Times.Once);
        }

        [Fact]
        public async Task IsRecoverCodeCorrectAsync_WhenCodeCorrect_ReturnsSuccess()
        {
            var login = "user";
            var code = 123456;

            var user = CreateTestUser(login);

            var hasherMock = new Mock<IHasherBase>();

            ulong hash = 12345678;

            user.RecoveryCodeHash = hash.ToString("x16");

            _userRepositoryMock.Setup(x => x.GetByLoginAsync(login)).ReturnsAsync(user);

            _hasherFactoryMock
                .Setup(x => x.GetHashMethod(user.HashMethod))
                .Returns(hasherMock.Object);

            hasherMock
                .Setup(x => x.HashValue(code.ToString(), user.RecoveryCodeSalt!))
                .Returns(hash);

            var service = CreateService();

            var result = await service.IsRecoverCodeCorrectAsync(code, login);

            Assert.True(result.IsSuccess);
            Assert.True(result.Value);
        }

        [Fact]
        public async Task IsRecoverCodeCorrectAsync_WhenCodeWrong_ReturnsFailure()
        {
            var login = "user";
            var code = 123456;

            var user = CreateTestUser(login);

            var hasherMock = new Mock<IHasherBase>();

            user.RecoveryCodeHash = "0000000000000000";

            _userRepositoryMock.Setup(x => x.GetByLoginAsync(login)).ReturnsAsync(user);

            _hasherFactoryMock
                .Setup(x => x.GetHashMethod(user.HashMethod))
                .Returns(hasherMock.Object);

            hasherMock
                .Setup(x => x.HashValue(code.ToString(), user.RecoveryCodeSalt!))
                .Returns(12345);

            var service = CreateService();

            var result = await service.IsRecoverCodeCorrectAsync(code, login);

            Assert.False(result.IsSuccess);
            Assert.Equal(PasswordRecoveryErrors.InvalidRecoveryCode, result.Error);
        }

        [Fact]
        public async Task ChangePasswordAsync_WhenCalled_EnablesRecoveryModeAndChangesPassword()
        {
            var login = "user";
            var password = "newPassword123!";

            _passwordChangerMock.SetupSet(x => x.IsRecoveryMode = true);

            _passwordChangerMock
                .Setup(x => x.ChangeUserPasswordAsync(login, password, ""))
                .ReturnsAsync(Result<int>.Success(123456));

            var service = CreateService();

            var result = await service.ChangePasswordAsync(login, password);

            Assert.True(result.IsSuccess);
            Assert.Equal(123456, result.Value);

            _passwordChangerMock.VerifySet(x => x.IsRecoveryMode = true, Times.Once);

            _passwordChangerMock.Verify(
                x => x.ChangeUserPasswordAsync(login, password, ""),
                Times.Once
            );
        }

        [Fact]
        public async Task IsUserExistAsync_WhenUserExists_ReturnsTrue()
        {
            var login = "user";

            _userRepositoryMock.Setup(x => x.ExistsByLoginAsync(login)).ReturnsAsync(true);

            var service = CreateService();

            var result = await service.IsUserExistAsync(login);

            Assert.True(result);

            _userRepositoryMock.Verify(x => x.ExistsByLoginAsync(login), Times.Once);
        }

        [Fact]
        public async Task IsUserExistAsync_WhenUserDoesNotExist_ReturnsFalse()
        {
            var login = "unknown";

            _userRepositoryMock.Setup(x => x.ExistsByLoginAsync(login)).ReturnsAsync(false);

            var service = CreateService();

            var result = await service.IsUserExistAsync(login);

            Assert.False(result);
        }

        private UsersInfo CreateTestUser(string login)
        {
            return new UsersInfo
            {
                Login = login,
                Username = login,
                HashMethod = HashMethod.Default,
                RecoveryCodeSalt = new byte[] { 1, 2, 3 },
                RecoveryCodeHash = "oldhash",
                PasswordHash = "passwordhash",
                Salt = new byte[] { 4, 5, 6 },
            };
        }
    }
}
