using Moq;
using WindowsDev.Application.Identity;
using WindowsDev.Application.Identity.Cryptography;
using WindowsDev.Application.Identity.PasswordChanger;
using WindowsDev.Application.Primitives;
using WindowsDev.Application.Users;
using WindowsDev.Domain.Entities;
using WindowsDev.Domain.Enums;
using WindowsDev.Domain.Messages.DialogsMessages.Errors;

namespace WindowsDev.Tests.Business.Profile;

public class ProfileServiceTest
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IUserSession> _userSessionMock;
    private readonly Mock<IPasswordChanger> _passwordChangerMock;
    private readonly Mock<IJwtService> _jwtServiceMock;

    public ProfileServiceTest()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _userSessionMock = new Mock<IUserSession>();
        _passwordChangerMock = new Mock<IPasswordChanger>();
        _jwtServiceMock = new Mock<IJwtService>();

        _passwordChangerMock.SetupProperty(x => x.IsRecoveryMode);
    }

    private ProfileService CreateService() =>
        new ProfileService(
            _userRepositoryMock.Object,
            _userSessionMock.Object,
            _passwordChangerMock.Object,
            _jwtServiceMock.Object
        );

    [Fact]
    public async Task ChangePasswordAsync_WhenPasswordsSame_ReturnsFailure()
    {
        var service = CreateService();

        var result = await service.ChangePasswordAsync("password", "password", "password");

        Assert.False(result.IsSuccess);
        Assert.Equal(ProfileErrors.NewPasswordSameAsCurrent, result.Error);

        _passwordChangerMock.Verify(
            x => x.ChangeUserPasswordAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()
            ),
            Times.Never
        );
    }

    [Fact]
    public async Task ChangePasswordAsync_WhenConfirmPasswordWrong_ReturnsFailure()
    {
        var service = CreateService();

        var result = await service.ChangePasswordAsync("old", "new", "different");

        Assert.False(result.IsSuccess);
        Assert.Equal(ProfileErrors.PasswordsDontMatch, result.Error);
    }

    [Fact]
    public async Task ChangePasswordAsync_WhenUserNotFound_ReturnsUserNotFound()
    {
        _userSessionMock.SetupGet(x => x.Login).Returns("user");

        _userRepositoryMock
            .Setup(x => x.ExistsByLoginAsync("user"))
            .ReturnsAsync(false);

        var service = CreateService();

        var result = await service.ChangePasswordAsync("old", "new", "new");

        Assert.False(result.IsSuccess);
        Assert.Equal(CommonErrors.UserNotFound, result.Error);

        _passwordChangerMock.Verify(
            x => x.ChangeUserPasswordAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()
            ),
            Times.Never
        );
    }

    [Fact]
    public async Task ChangePasswordAsync_WhenPasswordChangerReturnsError_ReturnsFailure()
    {
        _userSessionMock.SetupGet(x => x.Login).Returns("user");

        _userRepositoryMock
            .Setup(x => x.ExistsByLoginAsync("user"))
            .ReturnsAsync(true);

        _passwordChangerMock
            .Setup(x => x.ChangeUserPasswordAsync("user", "new", "old"))
            .ReturnsAsync(
                Result<(int, string, UserRefreshToken)>.Failure(
                    ProfileErrors.InvalidCurrentPassword
                )
            );

        var service = CreateService();

        var result = await service.ChangePasswordAsync("old", "new", "new");

        Assert.False(result.IsSuccess);
        Assert.Equal(ProfileErrors.InvalidCurrentPassword, result.Error);
    }

    [Fact]
    public async Task ChangePasswordAsync_WhenValid_ReturnsRecoveryCode()
    {
        _userSessionMock.SetupGet(x => x.Login).Returns("user");

        _userRepositoryMock
            .Setup(x => x.ExistsByLoginAsync("user"))
            .ReturnsAsync(true);

        var refreshToken = new UserRefreshToken
        {
            Id = 1,
            UserId = 42,
            Token = "refresh-token",
            IsRevoked = false,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
        };

        _passwordChangerMock
            .Setup(x => x.ChangeUserPasswordAsync("user", "new", "old"))
            .ReturnsAsync(
                Result<(int, string, UserRefreshToken)>.Success(
                    (123456, "some-value", refreshToken)
                )
            );

        var service = CreateService();

        var result = await service.ChangePasswordAsync("old", "new", "new");

        Assert.True(result.IsSuccess);
        Assert.Equal(123456, result.Value.Item1);
        Assert.Equal("some-value", result.Value.Item2);

        Assert.False(_passwordChangerMock.Object.IsRecoveryMode);

        _passwordChangerMock.Verify(
            x => x.ChangeUserPasswordAsync("user", "new", "old"),
            Times.Once
        );
    }

    [Fact]
    public async Task ChangeUsernameAsync_WhenUsernameSame_ReturnsFailure()
    {
        var service = CreateService();

        var result = await service.ChangeUsernameAsync("user", "user");

        Assert.False(result.IsSuccess);
        Assert.Equal(ProfileErrors.NewUsernameSameAsCurrent, result.Error);
    }

    [Fact]
    public async Task ChangeUsernameAsync_WhenUsernameTaken_ReturnsFailure()
    {
        _userRepositoryMock.Setup(x => x.ExistsByUsernameAsync("new")).ReturnsAsync(true);

        var service = CreateService();

        var result = await service.ChangeUsernameAsync("old", "new");

        Assert.False(result.IsSuccess);
        Assert.Equal(ProfileErrors.UsernameAlreadyTaken, result.Error);
    }

    [Fact]
    public async Task ChangeUsernameAsync_WhenUserNotFound_ReturnsFailure()
    {
        _userSessionMock.SetupGet(x => x.Login).Returns("login");

        _userRepositoryMock.Setup(x => x.ExistsByUsernameAsync("new")).ReturnsAsync(false);
        _userRepositoryMock
            .Setup(x => x.GetByLoginAsync("login"))
            .ReturnsAsync((UserInfo)null);

        var service = CreateService();

        var result = await service.ChangeUsernameAsync("old", "new");

        Assert.False(result.IsSuccess);
        Assert.Equal(CommonErrors.UserNotFound, result.Error);
    }

    [Fact]
    public async Task ChangeUsernameAsync_WhenValid_UpdatesUserAndReturnsJwt()
    {
        _userSessionMock.SetupGet(x => x.Login).Returns("login");

        var user = CreateUser();

        _userRepositoryMock.Setup(x => x.ExistsByUsernameAsync("new")).ReturnsAsync(false);
        _userRepositoryMock.Setup(x => x.GetByLoginAsync("login")).ReturnsAsync(user);
        _jwtServiceMock
            .Setup(x => x.GenerateToken(user.Id, user.Login, "new"))
            .Returns("jwt-token");

        var service = CreateService();

        var result = await service.ChangeUsernameAsync("old", "new");

        Assert.True(result.IsSuccess);
        Assert.Equal("jwt-token", result.Value);
        Assert.Equal("new", user.Username);
        _userRepositoryMock.Verify(x => x.UpdateAsync(user), Times.Once);
    }

    private UserInfo CreateUser() =>
        new UserInfo
        {
            Login = "user",
            Username = "old",
            HashMethod = HashMethod.Default,
            PasswordHash = "hash",
            Salt = new byte[] { 1, 2, 3 },
        };
}