using Moq;
using WindowsDev.Business.Primitives;
using WindowsDev.Business.Repositories.Interfaces;
using WindowsDev.Business.Services.PasswordManager;
using WindowsDev.Business.Services.PasswordManager.Hasher.Interfaces;
using WindowsDev.Business.Services.Profile;
using WindowsDev.Business.Services.UserManager;
using WindowsDev.Business.Services.UserManager.Interfaces;
using WindowsDev.Domain.DialogsMessages.Errors;
using WindowsDev.Domain.UsersModels;
using WindowsDev.Domain.UsersModels.Enums;

namespace WindowsDev.Tests.Business.Profile
{
    public class ProfileServiceTest
    {
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<IHasherFactory> _hasherFactoryMock;
        private readonly Mock<IPasswordChanger> _passwordChangerMock;

        private readonly ICurrentUserService _currentUserService;


        public ProfileServiceTest()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _hasherFactoryMock = new Mock<IHasherFactory>();

            _passwordChangerMock = new Mock<IPasswordChanger>();

            _passwordChangerMock
                .SetupProperty(x => x.IsRecoveryMode);

            _currentUserService = new CurrentUserService();
        }


        private ProfileService CreateService()
        {
            return new ProfileService(
                _userRepositoryMock.Object,
                _hasherFactoryMock.Object,
                _currentUserService,
                _passwordChangerMock.Object);
        }


        [Fact]
        public async Task ChangePasswordAsync_WhenPasswordsSame_ReturnsFailure()
        {
            var service = CreateService();

            var result = await service.ChangePasswordAsync(
                "password",
                "password",
                "password");


            Assert.False(result.IsSuccess);
            Assert.Equal(
                ProfileErrors.NewPasswordSameAsCurrent,
                result.Error);


            _passwordChangerMock.Verify(
                x => x.ChangeUserPasswordAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()),
                Times.Never);
        }



        [Fact]
        public async Task ChangePasswordAsync_WhenConfirmPasswordWrong_ReturnsFailure()
        {
            var service = CreateService();


            var result = await service.ChangePasswordAsync(
                "old",
                "new",
                "different");


            Assert.False(result.IsSuccess);
            Assert.Equal(
                ProfileErrors.PasswordsDontMatch,
                result.Error);
        }



        [Fact]
        public async Task ChangePasswordAsync_WhenUserNotFound_ThrowsException()
        {
            _currentUserService.Login = "user";


            _userRepositoryMock
                .Setup(x => x.GetByLoginAsync("user"))
                .ReturnsAsync((UsersInfo)null);


            var service = CreateService();


            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                service.ChangePasswordAsync(
                    "old",
                    "new",
                    "new"));
        }



        [Fact]
        public async Task ChangePasswordAsync_WhenPasswordChangerReturnsError_ReturnsFailure()
        {
            _currentUserService.Login = "user";


            _userRepositoryMock
                .Setup(x => x.GetByLoginAsync("user"))
                .ReturnsAsync(CreateUser());


            _passwordChangerMock
                .Setup(x => x.ChangeUserPasswordAsync(
                    "user",
                    "new",
                    "old"))
                .ReturnsAsync(
                    Result<int>.Failure(
                        ProfileErrors.InvalidCurrentPassword));


            var service = CreateService();


            var result = await service.ChangePasswordAsync(
                "old",
                "new",
                "new");


            Assert.False(result.IsSuccess);
            Assert.Equal(
                ProfileErrors.InvalidCurrentPassword,
                result.Error);
        }



        [Fact]
        public async Task ChangePasswordAsync_WhenValid_ReturnsRecoveryCode()
        {
            _currentUserService.Login = "user";


            _userRepositoryMock
                .Setup(x => x.GetByLoginAsync("user"))
                .ReturnsAsync(CreateUser());


            _passwordChangerMock
                .Setup(x => x.ChangeUserPasswordAsync(
                    "user",
                    "new",
                    "old"))
                .ReturnsAsync(
                    Result<int>.Success(123456));


            var service = CreateService();


            var result = await service.ChangePasswordAsync(
                "old",
                "new",
                "new");


            Assert.True(result.IsSuccess);
            Assert.Equal(123456, result.Value);


            Assert.False(_passwordChangerMock.Object.IsRecoveryMode);


            _passwordChangerMock.Verify(
                x => x.ChangeUserPasswordAsync(
                    "user",
                    "new",
                    "old"),
                Times.Once);
        }



        [Fact]
        public async Task ChangeUsernameAsync_WhenUsernameSame_ReturnsFailure()
        {
            var service = CreateService();


            var result = await service.ChangeUsernameAsync(
                "user",
                "user");


            Assert.False(result.IsSuccess);
            Assert.Equal(
                ProfileErrors.NewUsernameSameAsCurrent,
                result.Error);
        }



        [Fact]
        public async Task ChangeUsernameAsync_WhenUsernameTaken_ReturnsFailure()
        {
            _userRepositoryMock
                .Setup(x => x.ExistsByUsernameAsync("new"))
                .ReturnsAsync(true);


            var service = CreateService();


            var result = await service.ChangeUsernameAsync(
                "old",
                "new");


            Assert.False(result.IsSuccess);
            Assert.Equal(
                ProfileErrors.UsernamAlreadyTake,
                result.Error);
        }



        [Fact]
        public async Task ChangeUsernameAsync_WhenValid_UpdatesUser()
        {
            _currentUserService.Login = "login";


            var user = CreateUser();


            _userRepositoryMock
                .Setup(x => x.ExistsByUsernameAsync("new"))
                .ReturnsAsync(false);


            _userRepositoryMock
                .Setup(x => x.GetByLoginAsync("login"))
                .ReturnsAsync(user);



            var service = CreateService();


            var result = await service.ChangeUsernameAsync(
                "old",
                "new");



            Assert.True(result.IsSuccess);
            Assert.Equal(
                "new",
                _currentUserService.Username);


            Assert.Equal(
                "new",
                user.Username);


            _userRepositoryMock.Verify(
                x => x.UpdateAsync(user),
                Times.Once);
        }



        private UsersInfo CreateUser()
        {
            return new UsersInfo
            {
                Login = "user",
                Username = "old",
                HashMethod = HashMethod.Default,
                PasswordHash = "hash",
                Salt = new byte[] { 1, 2, 3 }
            };
        }
    }
}