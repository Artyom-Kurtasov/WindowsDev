using Moq;
using WindowsDev.Business.Repositories.Interfaces;
using WindowsDev.Business.Services.PasswordManager;
using WindowsDev.Business.Services.PasswordManager.Hasher.Interfaces;
using WindowsDev.Business.Services.UserManager.Interfaces;
using WindowsDev.Domain.DialogsMessages.Errors;
using WindowsDev.Domain.UsersModels;
using WindowsDev.Domain.UsersModels.Enums;

namespace WindowsDev.Tests.Business.PasswordManager
{
    public class PasswordChangerTests
    {
        private readonly Mock<ICurrentUserService> _currentUserMock;
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<IHasherFactory> _hasherFactoryMock;
        private readonly Mock<IHasherBase> _hasherMock;

        public PasswordChangerTests()
        {
            _currentUserMock = new Mock<ICurrentUserService>();
            _userRepositoryMock = new Mock<IUserRepository>();
            _hasherFactoryMock = new Mock<IHasherFactory>();
            _hasherMock = new Mock<IHasherBase>();

            _hasherFactoryMock
                .Setup(x => x.GetHashMethod(It.IsAny<HashMethod>()))
                .Returns(_hasherMock.Object);
        }

        private PasswordChanger CreateService()
        {
            return new PasswordChanger(
                _currentUserMock.Object,
                _userRepositoryMock.Object,
                _hasherFactoryMock.Object
            );
        }

        private UsersInfo CreateUser()
        {
            return new UsersInfo
            {
                Id = 1,
                Login = "test",
                PasswordHash = "0000000000001234",
                Salt = new byte[] { 1, 2, 3 },
                HashMethod = HashMethod.Default,
                Username = "Test User",
            };
        }

        [Fact]
        public async Task ChangeUserPassword_WhenPasswordCorrect_UpdatesUserAndReturnsRecoveryCode()
        {
            var user = CreateUser();
            var changer = CreateService();

            _userRepositoryMock.Setup(x => x.GetByLoginAsync("test")).ReturnsAsync(user);

            _hasherMock
                .Setup(x => x.HashValue(It.IsAny<string>(), It.IsAny<byte[]>()))
                .Returns(12345);

            _hasherMock
                .Setup(x => x.HashValue("oldPassword", user.Salt))
                .Returns(ConvertHexToUlong(user.PasswordHash));

            _hasherMock.Setup(x => x.GenerateSalt()).Returns(new byte[] { 5, 6, 7 });

            _userRepositoryMock.Setup(x => x.UpdateAsync(user)).Returns(Task.CompletedTask);

            var result = await changer.ChangeUserPasswordAsync(
                "test",
                "newPassword",
                "oldPassword"
            );

            Assert.True(result.IsSuccess);
            Assert.InRange(result.Value, 100000, 999999);

            Assert.Equal("0000000000003039", user.PasswordHash);
            Assert.Equal("0000000000003039", user.RecoveryCodeHash);

            _userRepositoryMock.Verify(x => x.UpdateAsync(user), Times.Once);
        }

        [Fact]
        public async Task ChangeUserPassword_WhenCurrentPasswordIncorrect_ReturnsFailure()
        {
            var user = CreateUser();
            var changer = CreateService();

            _userRepositoryMock.Setup(x => x.GetByLoginAsync("test")).ReturnsAsync(user);

            _hasherMock.Setup(x => x.HashValue("wrongPassword", user.Salt)).Returns(999);

            var result = await changer.ChangeUserPasswordAsync(
                "test",
                "newPassword",
                "wrongPassword"
            );

            Assert.True(result.IsFailure);

            Assert.Equal(ProfileErrors.InvalidCurrentPassword, result.Error);

            _userRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<UsersInfo>()), Times.Never);
        }

        [Fact]
        public async Task ChangeUserPassword_WhenRecoveryModeEnabled_DoesNotCheckCurrentPassword()
        {
            var user = CreateUser();
            var changer = CreateService();

            changer.IsRecoveryMode = true;

            _userRepositoryMock.Setup(x => x.GetByLoginAsync("test")).ReturnsAsync(user);

            _hasherMock.Setup(x => x.GenerateSalt()).Returns(new byte[] { 1, 2, 3 });

            _hasherMock
                .Setup(x => x.HashValue(It.IsAny<string>(), It.IsAny<byte[]>()))
                .Returns(123);

            _userRepositoryMock.Setup(x => x.UpdateAsync(user)).Returns(Task.CompletedTask);

            var result = await changer.ChangeUserPasswordAsync(
                "test",
                "newPassword",
                "wrongPassword"
            );

            Assert.True(result.IsSuccess);

            _hasherMock.Verify(x => x.HashValue("wrongPassword", user.Salt), Times.Never);
        }

        [Fact]
        public async Task ChangeUserPassword_WhenUserDoesNotExist_ThrowsException()
        {
            var changer = CreateService();

            _userRepositoryMock
                .Setup(x => x.GetByLoginAsync("unknown"))
                .ReturnsAsync((UsersInfo)null);

            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                changer.ChangeUserPasswordAsync("unknown", "password")
            );
        }

        [Fact]
        public void GenerateRecoveryCode_ReturnsSixDigitNumber()
        {
            var changer = CreateService();

            var code = changer.GenerateRecoveryCode();

            Assert.InRange(code, 100000, 999999);
        }

        private static ulong ConvertHexToUlong(string hex)
        {
            return ulong.Parse(hex, System.Globalization.NumberStyles.HexNumber);
        }
    }
}
