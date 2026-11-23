using Moq;
using System.Text;
using WindowsDev.Application.Identity.Authentication;
using WindowsDev.Application.Identity.Cryptography;
using WindowsDev.Application.Identity.Repositories;
using WindowsDev.Application.Users;
using WindowsDev.Domain.Entities;
using WindowsDev.Domain.Enums;
using WindowsDev.Domain.Messages.DialogsMessages.Errors;
using WindowsDev.Infrastructure.Security;

namespace WindowsDev.Tests.Business.Auth;

public class AuthorizationTest
{
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepositoryMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IJwtService> _jwtServiceMock;
    private readonly IHasherFactory _hasherFactory;

    public AuthorizationTest()
    {
        _refreshTokenRepositoryMock = new Mock<IRefreshTokenRepository>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _jwtServiceMock = new Mock<IJwtService>();

        var defaultHasher = new DefaultHasher();
        var simpleHasher = new SimpleHasher();

        _hasherFactory = new HasherFactory(defaultHasher, simpleHasher);
    }

    private Authentication CreateService()
    {
        return new Authentication(
            _refreshTokenRepositoryMock.Object,
            _userRepositoryMock.Object,
            _hasherFactory,
            _jwtServiceMock.Object
        );
    }

    [Theory]
    [InlineData("", "password")]
    [InlineData("login", "")]
    [InlineData(" ", "password")]
    [InlineData("login", " ")]
    public async Task Authenticate_WhenInputEmpty_ReturnsFailure(string login, string password)
    {
        var auth = CreateService();

        var result = await auth.Authenticate(login, password);

        Assert.False(result.IsSuccess);
        Assert.Equal(AuthErrors.InvalidCredentials, result.Error);

        _userRepositoryMock.Verify(x => x.GetByLoginAsync(It.IsAny<string>()), Times.Never);
        _jwtServiceMock.Verify(
            x => x.GenerateToken(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()),
            Times.Never
        );
        _refreshTokenRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<UserRefreshToken>()),
            Times.Never
        );
    }

    [Fact]
    public async Task Authenticate_WhenUserNotFound_ReturnsFailure()
    {
        _userRepositoryMock
            .Setup(x => x.GetByLoginAsync("login"))
            .ReturnsAsync((UserInfo?)null);

        var auth = CreateService();

        var result = await auth.Authenticate("login", "password");

        Assert.False(result.IsSuccess);
        Assert.Equal(AuthErrors.InvalidCredentials, result.Error);

        _userRepositoryMock.Verify(x => x.GetByLoginAsync("login"), Times.Once);
        _jwtServiceMock.Verify(
            x => x.GenerateToken(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()),
            Times.Never
        );
    }

    [Fact]
    public async Task Authenticate_WhenPasswordIncorrect_ReturnsFailure()
    {
        var salt = Encoding.UTF8.GetBytes("salt");

        var hasher = _hasherFactory.GetHashMethod(HashMethod.Default);
        var correctHash = hasher.HashValue("correct", salt).ToString("x16");

        var user = new UserInfo
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

        var result = await auth.Authenticate("admin", "wrong");

        Assert.False(result.IsSuccess);
        Assert.Equal(AuthErrors.InvalidCredentials, result.Error);

        _jwtServiceMock.Verify(
            x => x.GenerateToken(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()),
            Times.Never
        );
        _refreshTokenRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<UserRefreshToken>()),
            Times.Never
        );
    }

    [Fact]
    public async Task Authenticate_WhenCorrect_ReturnsTokenAndSavesRefreshToken()
    {
        var salt = Encoding.UTF8.GetBytes("salt");

        var hasher = _hasherFactory.GetHashMethod(HashMethod.Default);
        var hash = hasher.HashValue("password", salt).ToString("x16");

        var user = new UserInfo
        {
            Id = 1,
            Login = "admin",
            Username = "Artyom",
            Salt = salt,
            PasswordHash = hash,
            HashMethod = HashMethod.Default,
        };

        var jwt = "jwt-token";
        var refresh = new UserRefreshToken
        {
            Id = 10,
            UserId = user.Id,
            Token = "refresh-token",
            ExpiresAt = DateTime.UtcNow.AddDays(7),
        };

        _userRepositoryMock.Setup(x => x.GetByLoginAsync("admin")).ReturnsAsync(user);
        _jwtServiceMock
            .Setup(x => x.GenerateToken(user.Id, user.Login, user.Username))
            .Returns(jwt);
        _jwtServiceMock
            .Setup(x => x.GenerateRefreshToken(user.Id))
            .Returns(refresh);

        var auth = CreateService();

        var result = await auth.Authenticate("admin", "password");

        Assert.True(result.IsSuccess);
        Assert.Equal(jwt, result.Value.Item1);
        Assert.Equal(refresh, result.Value.Item2);

        _jwtServiceMock.Verify(
            x => x.GenerateToken(user.Id, user.Login, user.Username),
            Times.Once
        );
        _jwtServiceMock.Verify(x => x.GenerateRefreshToken(user.Id), Times.Once);
        _refreshTokenRepositoryMock.Verify(x => x.AddAsync(refresh), Times.Once);
    }
}