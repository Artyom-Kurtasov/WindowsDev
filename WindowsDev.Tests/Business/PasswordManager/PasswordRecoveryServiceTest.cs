using Moq;
using WindowsDev.Business.Repositories.Interfaces;
using WindowsDev.Business.Services.PasswordManager.Hasher.Interfaces;
using WindowsDev.Business.Services.PasswordManager.PasswordRecovery;
using WindowsDev.Domain.UsersModels;
using WindowsDev.Domain.UsersModels.Enums;

namespace WindowsDev.Tests.Business.PasswordManager
{
    public class PasswordRecoveryServiceTest
    {
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<IHasherFactory> _hasherFactoryMock;

        public PasswordRecoveryServiceTest()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _hasherFactoryMock = new Mock<IHasherFactory>();
        }

        private PasswordRecoveryService CreateService()
        {
            return new PasswordRecoveryService(
                _hasherFactoryMock.Object,
                _userRepositoryMock.Object);
        }

        [Fact]
        public async Task IsRecoverCodeCorrect_WhenUserNotFound_ReturnsFalse()
        {
            var login = "nonexistent";
            var recoveryCode = 123456;

            _userRepositoryMock
                .Setup(x => x.GetByLoginAsync(login))
                .ReturnsAsync((UsersInfo?)null);

            var service = CreateService();

            var result = await service.IsRecoverCodeCorrect(recoveryCode, login);

            Assert.False(result);
            _userRepositoryMock.Verify(x => x.GetByLoginAsync(login), Times.Once);
        }

        [Fact]
        public async Task IsRecoverCodeCorrect_WhenCodeIsCorrect_ReturnsTrue()
        {
            var login = "testuser";
            var recoveryCode = 123456;
            var user = CreateTestUser(login);
            ulong hashedCodeUlong = 12345678;

            var hasherMock = new Mock<IHasherBase>();

            user.RecoveryCodeHash = hashedCodeUlong.ToString("x16");

            _userRepositoryMock
                .Setup(x => x.GetByLoginAsync(login))
                .ReturnsAsync(user);

            _hasherFactoryMock
                .Setup(x => x.GetHashMethod(user.HashMethod))
                .Returns(hasherMock.Object);

            hasherMock
                .Setup(x => x.HashPassword(recoveryCode.ToString(), user.RecoveryCodeSalt!))
                .Returns(hashedCodeUlong);

            var service = CreateService();

            var result = await service.IsRecoverCodeCorrect(recoveryCode, login);

            Assert.True(result);
            _userRepositoryMock.Verify(x => x.GetByLoginAsync(login), Times.Once);
        }

        [Fact]
        public async Task IsRecoverCodeCorrect_WhenCodeIsIncorrect_ReturnsFalse()
        {
            var login = "testuser";
            var recoveryCode = 123456;
            var user = CreateTestUser(login);
            ulong hashedCode = 12345678;
            ulong wrongHashedCode = 87654321;

            var hasherMock = new Mock<IHasherBase>();

            user.RecoveryCodeHash = hashedCode.ToString("x16");

            _userRepositoryMock
                .Setup(x => x.GetByLoginAsync(login))
                .ReturnsAsync(user);

            _hasherFactoryMock
                .Setup(x => x.GetHashMethod(user.HashMethod))
                .Returns(hasherMock.Object);

            hasherMock
                .Setup(x => x.HashPassword(recoveryCode.ToString(), user.RecoveryCodeSalt!))
                .Returns(wrongHashedCode);

            var service = CreateService();

            var result = await service.IsRecoverCodeCorrect(recoveryCode, login);

            Assert.False(result);
            _userRepositoryMock.Verify(x => x.GetByLoginAsync(login), Times.Once);
        }

        [Fact]
        public async Task ChangePasswordAsync_WhenUserNotFound_ReturnsMinusOne()
        {
            var login = "nonexistent";
            var password = "newPassword123!";

            _userRepositoryMock
                .Setup(x => x.GetByLoginAsync(login))
                .ReturnsAsync((UsersInfo?)null);

            var service = CreateService();

            var result = await service.ChangePasswordAsync(login, password);

            Assert.Equal(-1, result);
            _userRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<UsersInfo>()), Times.Never);
        }

        [Fact]
        public async Task ChangePasswordAsync_WhenUserExists_UpdatesUserAndReturnsRecoveryCode()
        {
            var login = "testuser";
            var password = "newPassword123!";
            var user = CreateTestUser(login);
            var passwordSalt = new byte[] { 1, 2, 3 };
            var recoveryCodeSalt = new byte[] { 4, 5, 6 };
            ulong recoveryCodeHash = 12345678;
            ulong passwordHash = 87654321;
            var callCount = 0;

            var hasherMock = new Mock<IHasherBase>();

            _userRepositoryMock
                .Setup(x => x.GetByLoginAsync(login))
                .ReturnsAsync(user);

            _hasherFactoryMock
                .Setup(x => x.GetHashMethod(user.HashMethod))
                .Returns(hasherMock.Object);

            hasherMock
                .Setup(x => x.GenerateSalt())
                .Returns(() =>
                {
                    callCount++;
                    return callCount == 1 ? passwordSalt : recoveryCodeSalt;
                });

            hasherMock
                .Setup(x => x.HashPassword(password, passwordSalt))
                .Returns(passwordHash);

            hasherMock
                .Setup(x => x.HashPassword(It.IsAny<string>(), recoveryCodeSalt))
                .Returns(recoveryCodeHash);

            var service = CreateService();

            var result = await service.ChangePasswordAsync(login, password);

            Assert.InRange(result, 100000, 999999);
            Assert.Equal(passwordSalt, user.Salt);
            Assert.Equal(recoveryCodeSalt, user.RecoveryCodeSalt);
            Assert.Equal(passwordHash.ToString("x16"), user.PasswordHash);
            Assert.Equal(recoveryCodeHash.ToString("x16"), user.RecoveryCodeHash);

            _userRepositoryMock.Verify(x => x.UpdateAsync(user), Times.Once);
        }

        private UsersInfo CreateTestUser(string login)
        {
            return new UsersInfo
            {
                Login = login,
                Username = login,
                HashMethod = HashMethod.Default,
                Salt = new byte[] { 1, 2, 3, 4 },
                PasswordHash = "",
                RecoveryCodeHash = "",
                RecoveryCodeSalt = new byte[] { 5, 6, 7, 8 }
            };
        }
    }
}