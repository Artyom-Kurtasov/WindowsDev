using Moq;
using WindowsDev.Application.Identity.Cryptography;
using WindowsDev.Application.Identity.PasswordChanger;
using WindowsDev.Application.Users;
using WindowsDev.Domain.Entities;
using WindowsDev.Domain.Enums;
using WindowsDev.Domain.Messages.DialogsMessages.Errors;

namespace WindowsDev.Tests.Business.PasswordManager;

public class PasswordChangerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IHasherFactory> _hasherFactoryMock;
    private readonly Mock<IHasherBase> _hasherMock;
    private readonly Mock<IJwtService> _jwtServiceMock;

    public PasswordChangerTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _hasherFactoryMock = new Mock<IHasherFactory>();
        _hasherMock = new Mock<IHasherBase>();
        _jwtServiceMock = new Mock<IJwtService>();

        _hasherFactoryMock
            .Setup(x => x.GetHashMethod(It.IsAny<HashMethod>()))
            .Returns(_hasherMock.Object);
    }

    private PasswordChanger CreateService()
    {
        return new PasswordChanger(
            _jwtServiceMock.Object,
            _userRepositoryMock.Object,
            _hasherFactoryMock.Object
        );
    }

    private UserInfo CreateUser()
    {
        return new UserInfo
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
            .Setup(x => x.HashValue("oldPassword", user.Salt))
            .Returns(ConvertHexToUlong(user.PasswordHash));

        _hasherMock.Setup(x => x.GenerateSalt()).Returns(new byte[] { 5, 6, 7 });
        _hasherMock
            .Setup(x => x.HashValue("newPassword", It.IsAny<byte[]>()))
            .Returns(123456UL);
        _hasherMock
            .Setup(x => x.HashValue(It.Is<string>(s => IsRecoveryCode(s)), It.IsAny<byte[]>()))
            .Returns(654321UL);

        _jwtServiceMock
            .Setup(x => x.GenerateToken(user.Id, user.Login, user.Username))
            .Returns("jwt-token");
        var refreshToken = new UserRefreshToken
        {
            Id = 10,
            UserId = user.Id,
            Token = "refresh-token",
            ExpiresAt = DateTime.UtcNow.AddDays(7),
        };
        _jwtServiceMock
            .Setup(x => x.GenerateRefreshToken(user.Id))
            .Returns(refreshToken);

        _userRepositoryMock.Setup(x => x.UpdateAsync(user)).Returns(Task.CompletedTask);

        var result = await changer.ChangeUserPasswordAsync(
            "test",
            "newPassword",
            "oldPassword"
        );

        Assert.True(result.IsSuccess);
        Assert.InRange(result.Value.Item1, 100000, 999999);
        Assert.Equal("jwt-token", result.Value.Item2);
        Assert.Equal(refreshToken, result.Value.Item3);

        Assert.Equal("000000000001e240", user.PasswordHash); // 123456 hex x16
        Assert.Equal("000000000009fbf1", user.RecoveryCodeHash); // 654321 hex x16

        _userRepositoryMock.Verify(x => x.UpdateAsync(user), Times.Once);
    }

    [Fact]
    public async Task ChangeUserPassword_WhenCurrentPasswordIncorrect_ReturnsFailure()
    {
        var user = CreateUser();
        var changer = CreateService();

        _userRepositoryMock.Setup(x => x.GetByLoginAsync("test")).ReturnsAsync(user);

        _hasherMock.Setup(x => x.HashValue("wrongPassword", user.Salt)).Returns(999UL);

        var result = await changer.ChangeUserPasswordAsync(
            "test",
            "newPassword",
            "wrongPassword"
        );

        Assert.True(result.IsFailure);
        Assert.Equal(ProfileErrors.InvalidCurrentPassword, result.Error);

        _userRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<UserInfo>()), Times.Never);
        _jwtServiceMock.Verify(
            x => x.GenerateToken(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()),
            Times.Never
        );
        _jwtServiceMock.Verify(x => x.GenerateRefreshToken(It.IsAny<int>()), Times.Never);
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
            .Returns(123UL);

        _jwtServiceMock
            .Setup(x => x.GenerateToken(user.Id, user.Login, user.Username))
            .Returns("jwt-token");
        var refreshToken = new UserRefreshToken
        {
            Id = 10,
            UserId = user.Id,
            Token = "refresh-token",
            ExpiresAt = DateTime.UtcNow.AddDays(7),
        };
        _jwtServiceMock
            .Setup(x => x.GenerateRefreshToken(user.Id))
            .Returns(refreshToken);

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
    public async Task ChangeUserPassword_WhenUserDoesNotExist_ReturnsFailure()
    {
        var changer = CreateService();

        _userRepositoryMock
            .Setup(x => x.GetByLoginAsync("unknown"))
            .ReturnsAsync((UserInfo?)null);

        var result = await changer.ChangeUserPasswordAsync("unknown", "password");

        Assert.True(result.IsFailure);
        Assert.Equal(CommonErrors.UserNotFound, result.Error);

        _userRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<UserInfo>()), Times.Never);
    }

    [Fact]
    public void GenerateRecoveryCode_ReturnsSixDigitNumber()
    {
        var changer = CreateService();

        var code = changer.GenerateRecoveryCode();

        Assert.InRange(code, 100000, 999999);
    }

    private static bool IsRecoveryCode(string value)
        => int.TryParse(value, out var v) && v >= 100000 && v <= 999999;

    private static ulong ConvertHexToUlong(string hex)
    {
        return ulong.Parse(hex, System.Globalization.NumberStyles.HexNumber);
    }
}