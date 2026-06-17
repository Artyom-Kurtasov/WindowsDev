using Moq;
using WindowsDev.Business.Repositories.Interfaces;
using WindowsDev.Business.Services.PasswordManager.Hasher.Interfaces;
using WindowsDev.Business.Services.Profile;
using WindowsDev.Business.Services.UserManager;
using WindowsDev.Business.Services.UserManager.Interfaces;
using WindowsDev.Domain.UsersModels;
using WindowsDev.Domain.UsersModels.Enums;

namespace WindowsDev.Tests.Business.Profile
{
    public class ProfileServiceTest
    {
        private readonly ICurrentUserService _userService;
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<IHasherFactory> _hasherFactoryMock;

        public ProfileServiceTest()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _hasherFactoryMock = new Mock<IHasherFactory>();

            _userService = new CurrentUserService();
        }

        private ProfileService CreateService()
        {
            return new ProfileService(
                _userRepositoryMock.Object,
                _hasherFactoryMock.Object,
                _userService);
        }

        [Fact]
        public async Task ChangePasswordAsync_WhenNewPasswordSameAsCurrent_ThrowsException()
        {
            var service = CreateService();

            await Assert.ThrowsAsync<Exception>(
                () => service.ChangePasswordAsync("password", "password", "password"));

            _userRepositoryMock.Verify(x => x.GetByLoginAsync(It.IsAny<string>()), Times.Never);
            _userRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<UsersInfo>()), Times.Never);
        }

        [Fact]
        public async Task ChangePasswordAsync_WhenNewPasswordDoesNotMatchConfirm_ThrowsException()
        {
            var service = CreateService();

            await Assert.ThrowsAsync<Exception>(
                () => service.ChangePasswordAsync("currentPass", "newPass123", "differentPass"));

            _userRepositoryMock.Verify(x => x.GetByLoginAsync(It.IsAny<string>()), Times.Never);
            _userRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<UsersInfo>()), Times.Never);
        }

        [Fact]
        public async Task ChangePasswordAsync_WhenCurrentPasswordIsIncorrect_ThrowsException()
        {
            var userLogin = "testuser";
            var currentPassword = "currentPass";
            var newPassword = "newPass123";
            var confirmPassword = "newPass123";
            var user = CreateTestUser(userLogin);
            ulong wrongHash = 13958235;

            var hasherMock = new Mock<IHasherBase>();

            _userService.Login = userLogin;
            user.PasswordHash = "differenthashvalue0000000000000000";

            _userRepositoryMock
                .Setup(x => x.GetByLoginAsync(userLogin))
                .ReturnsAsync(user);

            _hasherFactoryMock
                .Setup(x => x.GetHashMethod(user.HashMethod))
                .Returns(hasherMock.Object);

            hasherMock
                .Setup(x => x.HashPassword(currentPassword, user.Salt))
                .Returns(wrongHash);

            var service = CreateService();

            await Assert.ThrowsAsync<Exception>(
                () => service.ChangePasswordAsync(currentPassword, newPassword, confirmPassword));

            _userRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<UsersInfo>()), Times.Never);
        }

        [Fact]
        public async Task ChangePasswordAsync_WhenAllValid_UpdatesPasswordSuccessfully()
        {
            var userLogin = "testuser";
            var currentPassword = "currentPass";
            var newPassword = "newPass123";
            var confirmPassword = "newPass123";
            var user = CreateTestUser(userLogin);
            ulong currentHash = 12345678;
            var newSalt = new byte[] { 1, 2, 3 };
            ulong newHash = 87654321;

            var hasherMock = new Mock<IHasherBase>();

            _userService.Login = userLogin;
            user.PasswordHash = currentHash.ToString("x16");

            _userRepositoryMock
                .Setup(x => x.GetByLoginAsync(userLogin))
                .ReturnsAsync(user);

            _hasherFactoryMock
                .Setup(x => x.GetHashMethod(user.HashMethod))
                .Returns(hasherMock.Object);

            hasherMock
                .Setup(x => x.HashPassword(currentPassword, user.Salt))
                .Returns(currentHash);

            hasherMock
                .Setup(x => x.GenerateSalt())
                .Returns(newSalt);

            hasherMock
                .Setup(x => x.HashPassword(newPassword, newSalt))
                .Returns(newHash);

            var service = CreateService();

            await service.ChangePasswordAsync(currentPassword, newPassword, confirmPassword);

            Assert.Equal(newSalt, user.Salt);
            Assert.Equal(newHash.ToString("x16"), user.PasswordHash);

            _userRepositoryMock.Verify(x => x.UpdateAsync(user), Times.Once);
        }

        [Fact]
        public async Task ChangeUsernameAsync_WhenCurrentUsernameSameAsNew_ThrowsException()
        {
            var username = "sameuser";
            var service = CreateService();

            await Assert.ThrowsAsync<Exception>(
                () => service.ChangeUsernameAsync(username, username));

            _userRepositoryMock.Verify(x => x.ExistsByUsernameAsync(It.IsAny<string>()), Times.Never);
            _userRepositoryMock.Verify(x => x.GetByLoginAsync(It.IsAny<string>()), Times.Never);
            _userRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<UsersInfo>()), Times.Never);
        }

        [Fact]
        public async Task ChangeUsernameAsync_WhenNewUsernameAlreadyExists_ThrowsException()
        {
            var currentUsername = "currentuser";
            var newUsername = "existinguser";

            _userRepositoryMock
                .Setup(x => x.ExistsByUsernameAsync(newUsername))
                .ReturnsAsync(true);

            var service = CreateService();

            await Assert.ThrowsAsync<Exception>(
                () => service.ChangeUsernameAsync(currentUsername, newUsername));

            _userRepositoryMock.Verify(x => x.GetByLoginAsync(It.IsAny<string>()), Times.Never);
            _userRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<UsersInfo>()), Times.Never);
        }

        [Fact]
        public async Task ChangeUsernameAsync_WhenValid_UpdatesUsernameSuccessfully()
        {
            var currentUsername = "olduser";
            var newUsername = "newuser";
            var userLogin = "userlogin";
            var user = CreateTestUser(userLogin);
            user.Username = currentUsername;

            _userService.Login = userLogin;

            _userRepositoryMock
                .Setup(x => x.ExistsByUsernameAsync(newUsername))
                .ReturnsAsync(false);

            _userRepositoryMock
                .Setup(x => x.GetByLoginAsync(userLogin))
                .ReturnsAsync(user);

            var service = CreateService();

            await service.ChangeUsernameAsync(currentUsername, newUsername);

            Assert.Equal(newUsername, _userService.Username);
            Assert.Equal(newUsername, user.Username);

            _userRepositoryMock.Verify(x => x.UpdateAsync(user), Times.Once);
        }

        [Fact]
        public async Task ChangeUsernameAsync_WhenUserNotFound_ThrowsException()
        {
            var currentUsername = "olduser";
            var newUsername = "newuser";
            var userLogin = "nonexistent";

            _userService.Login = userLogin;

            _userRepositoryMock
                .Setup(x => x.ExistsByUsernameAsync(newUsername))
                .ReturnsAsync(false);

            _userRepositoryMock
                .Setup(x => x.GetByLoginAsync(userLogin))
                .ReturnsAsync((UsersInfo?)null);

            var service = CreateService();

            await Assert.ThrowsAsync<Exception>(() =>
                service.ChangeUsernameAsync(currentUsername, newUsername));

            Assert.Equal(string.Empty, _userService.Username);
            _userRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<UsersInfo>()), Times.Never);
        }

        private UsersInfo CreateTestUser(string login)
        {
            return new UsersInfo
            {
                Login = login,
                Username = login,
                HashMethod = HashMethod.Default,
                Salt = new byte[] { 1, 2, 3, 4 },
                PasswordHash = ""
            };
        }
    }
}