using Moq;
using WindowsDev.Application.Identity.Cryptography;
using WindowsDev.Application.Identity.PasswordChanger;
using WindowsDev.Application.Identity.Repositories;
using WindowsDev.Application.Primitives;
using WindowsDev.Application.Users;
using WindowsDev.Domain.Entities;
using WindowsDev.Domain.Enums;
using RegistrationService = WindowsDev.Application.Identity.Registration.Registration;

namespace WindowsDev.Tests.Business.Registration;

public class RegistrationTest
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IDefaultHasher> _defaultHasherMock;
    private readonly Mock<IPasswordChanger> _passwordChangerMock;
    private readonly Mock<IJwtService> _jwtServiceMock;
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepositoryMock;

    public RegistrationTest()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _defaultHasherMock = new Mock<IDefaultHasher>();
        _passwordChangerMock = new Mock<IPasswordChanger>();
        _jwtServiceMock = new Mock<IJwtService>();
        _refreshTokenRepositoryMock = new Mock<IRefreshTokenRepository>();

        _defaultHasherMock.Setup(x => x.GenerateSalt()).Returns(new byte[] { 1, 2, 3 });
        _defaultHasherMock
            .Setup(x => x.HashValue(It.IsAny<string>(), It.IsAny<byte[]>()))
            .Returns(123456789UL);

        _userRepositoryMock
            .Setup(x => x.ExistsByLoginAsync(It.IsAny<string>()))
            .ReturnsAsync(false);
        _userRepositoryMock
            .Setup(x => x.ExistsByUsernameAsync(It.IsAny<string>()))
            .ReturnsAsync(false);

        _jwtServiceMock
            .Setup(x => x.GenerateToken(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns("jwt-token");
        _jwtServiceMock
            .Setup(x => x.GenerateRefreshToken(It.IsAny<int>()))
            .Returns((int userId) => new UserRefreshToken
            {
                Id = 1,
                UserId = userId,
                Token = "refresh-token",
                IsRevoked = false,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
            });
        _refreshTokenRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<UserRefreshToken>()))
            .Returns(Task.CompletedTask);
    }

    private RegistrationService CreateService() =>
        new RegistrationService(
            _userRepositoryMock.Object,
            _defaultHasherMock.Object,
            _passwordChangerMock.Object,
            _jwtServiceMock.Object,
            _refreshTokenRepositoryMock.Object
        );

    [Fact]
    public async Task Register_WhenAllCorrect_ReturnsRecoveryCodeAndTokens()
    {
        const int expectedRecoveryCode = 123456;
        _passwordChangerMock
            .Setup(x => x.GenerateRecoveryCode())
            .Returns(expectedRecoveryCode);

        var registration = CreateService();

        var result = await registration.Register("password", "login", "username");

        Assert.True(result.IsSuccess);
        Assert.Equal(expectedRecoveryCode, result.Value.Item1);
        Assert.Equal("jwt-token", result.Value.Item2);
        Assert.Equal("refresh-token", result.Value.Item3.Token);

        _userRepositoryMock.Verify(
            x =>
                x.AddAsync(
                    It.Is<UserInfo>(u =>
                        u.Login == "login"
                        && u.Username == "username"
                        && u.PasswordHash != null
                        && u.Salt != null
                        && u.RecoveryCodeHash != null
                        && u.RecoveryCodeSalt != null
                    )
                ),
            Times.Once
        );

        _refreshTokenRepositoryMock.Verify(
            x => x.AddAsync(It.Is<UserRefreshToken>(t => t.Token == "refresh-token")),
            Times.Once
        );
    }

    [Fact]
    public async Task Register_WhenAllCorrect_GeneratesPasswordAndRecoveryCodeHashes()
    {
        const int expectedRecoveryCode = 456789;
        _passwordChangerMock
            .Setup(x => x.GenerateRecoveryCode())
            .Returns(expectedRecoveryCode);

        var registration = CreateService();

        var result = await registration.Register("password", "login", "username");

        Assert.True(result.IsSuccess);
        Assert.Equal(expectedRecoveryCode, result.Value.Item1);

        _userRepositoryMock.Verify(
            x =>
                x.AddAsync(
                    It.Is<UserInfo>(u =>
                        !string.IsNullOrEmpty(u.PasswordHash)
                        && u.Salt != null
                        && !string.IsNullOrEmpty(u.RecoveryCodeHash)
                        && u.RecoveryCodeSalt != null
                    )
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task Register_WhenRepositoryThrows_PropagatesException()
    {
        _passwordChangerMock.Setup(x => x.GenerateRecoveryCode()).Returns(123456);

        _userRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<UserInfo>()))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        var registration = CreateService();

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            registration.Register("password", "login", "username")
        );
    }

    [Fact]
    public async Task Register_WhenLoginTaken_ReturnsFailure()
    {
        _userRepositoryMock
            .Setup(x => x.ExistsByLoginAsync("login"))
            .ReturnsAsync(true);

        var registration = CreateService();

        var result = await registration.Register("password", "login", "username");

        Assert.False(result.IsSuccess);
        _userRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<UserInfo>()),
            Times.Never
        );
    }

    [Fact]
    public async Task Register_WhenUsernameTaken_ReturnsFailure()
    {
        _userRepositoryMock
            .Setup(x => x.ExistsByUsernameAsync("username"))
            .ReturnsAsync(true);

        var registration = CreateService();

        var result = await registration.Register("password", "login", "username");

        Assert.False(result.IsSuccess);
        _userRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<UserInfo>()),
            Times.Never
        );
    }

    [Fact]
    public async Task Register_WhenAllCorrect_UserHasDefaultHashMethod()
    {
        _passwordChangerMock.Setup(x => x.GenerateRecoveryCode()).Returns(111222);

        var registration = CreateService();

        var result = await registration.Register("password", "login", "username");

        Assert.True(result.IsSuccess);

        _userRepositoryMock.Verify(
            x => x.AddAsync(It.Is<UserInfo>(u => u.HashMethod == HashMethod.Default)),
            Times.Once
        );
    }
}