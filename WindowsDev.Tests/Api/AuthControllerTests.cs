using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using WindowsDev.Api.Controllers;
using WindowsDev.Api.ProblemDetails;
using WindowsDev.Application.Identity.Authentication;
using WindowsDev.Application.Identity.Registration;
using WindowsDev.Application.Primitives;
using WindowsDev.Contracts.Api.Identity;
using WindowsDev.Domain.Entities;
using WindowsDev.Domain.Messages.DialogsMessages.Errors;
using WindowsDev.Domain.Messages.DialogsMessages.Warnings;

namespace WindowsDev.Tests.Api;

public class AuthControllerTest
{
    private readonly Mock<IAuthentication> _authenticationMock;
    private readonly Mock<IRegistration> _registrationMock;
    private readonly Mock<ILogger<AuthController>> _loggerMock;

    public AuthControllerTest()
    {
        _authenticationMock = new Mock<IAuthentication>();
        _registrationMock = new Mock<IRegistration>();
        _loggerMock = new Mock<ILogger<AuthController>>();
    }

    private AuthController CreateController()
    {
        var controller = new AuthController(
            _authenticationMock.Object,
            _registrationMock.Object,
            _loggerMock.Object
        );

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        return controller;
    }

    private static UserRefreshToken CreateRefreshToken(string token, int id = 1) =>
        new UserRefreshToken
        {
            Id = id,
            UserId = 42,
            Token = token,
            IsRevoked = false,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
        };

    [Fact]
    public async Task RegisterAsync_WhenSuccess_ReturnsOkWithResponse()
    {
        var refreshToken = CreateRefreshToken("refresh-token");
        var request = new UserRegisterRequest
        {
            Login = "login",
            Password = "password",
            Username = "username",
        };

        _registrationMock
            .Setup(x => x.Register(request.Password, request.Login, request.Username))
            .ReturnsAsync(Result<(int, string, UserRefreshToken)>.Success((123456, "jwt-token", refreshToken)));

        var controller = CreateController();

        var result = await controller.RegisterAsync(request);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<UserRegisterResponse>(ok.Value);
        Assert.Equal(123456, response.RecoveryCode);
        Assert.Equal("jwt-token", response.JwtToken);
        Assert.Same(refreshToken, response.RefreshToken);
    }

    [Fact]
    public async Task RegisterAsync_WhenLoginTaken_ReturnsConflict()
    {
        var request = new UserRegisterRequest
        {
            Login = "taken-login",
            Password = "password",
            Username = "username",
        };

        _registrationMock
            .Setup(x => x.Register(request.Password, request.Login, request.Username))
            .ReturnsAsync(Result<(int, string, UserRefreshToken)>.Failure(AuthDialogWarnings.LoginTaken));

        var controller = CreateController();

        var result = await controller.RegisterAsync(request);

        var objectResult = Assert.IsAssignableFrom<ObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status409Conflict, objectResult.StatusCode);
    }

    [Fact]
    public async Task RegisterAsync_WhenUsernameTaken_ReturnsConflict()
    {
        var request = new UserRegisterRequest
        {
            Login = "login",
            Password = "password",
            Username = "taken-username",
        };

        _registrationMock
            .Setup(x => x.Register(request.Password, request.Login, request.Username))
            .ReturnsAsync(Result<(int, string, UserRefreshToken)>.Failure(AuthDialogWarnings.UsernameTaken));

        var controller = CreateController();

        var result = await controller.RegisterAsync(request);

        var objectResult = Assert.IsAssignableFrom<ObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status409Conflict, objectResult.StatusCode);
    }

    [Fact]
    public async Task RegisterAsync_WhenUnexpectedError_ReturnsInternalServerError()
    {
        var request = new UserRegisterRequest
        {
            Login = "login",
            Password = "password",
            Username = "username",
        };

        _registrationMock
            .Setup(x => x.Register(request.Password, request.Login, request.Username))
            .ReturnsAsync(Result<(int, string, UserRefreshToken)>.Failure(CommonErrors.UnexpectedError));

        var controller = CreateController();

        var result = await controller.RegisterAsync(request);

        var statusCode = Assert.IsType<StatusCodeResult>(result.Result);
        Assert.Equal(StatusCodes.Status500InternalServerError, statusCode.StatusCode);
    }

    [Fact]
    public async Task LoginAsync_WhenSuccess_ReturnsOkWithTokens()
    {
        var refreshToken = CreateRefreshToken("refresh-token");
        var request = new UserLoginRequest { Login = "login", Password = "password" };

        _authenticationMock
            .Setup(x => x.Authenticate(request.Login, request.Password))
            .ReturnsAsync(Result<(string, UserRefreshToken)>.Success(("access-token", refreshToken)));

        var controller = CreateController();

        var result = await controller.LoginAsync(request);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<UserLoginResponse>(ok.Value);
        Assert.Equal("access-token", response.AccessToken);
        Assert.Same(refreshToken, response.RefreshToken);
    }

    [Fact]
    public async Task LoginAsync_WhenInvalidCredentials_ReturnsUnauthorizedProblem()
    {
        var request = new UserLoginRequest { Login = "login", Password = "wrong" };

        _authenticationMock
            .Setup(x => x.Authenticate(request.Login, request.Password))
            .ReturnsAsync(Result<(string, UserRefreshToken)>.Failure(AuthErrors.InvalidCredentials));

        var controller = CreateController();

        var result = await controller.LoginAsync(request);

        var objectResult = Assert.IsAssignableFrom<ObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status401Unauthorized, objectResult.StatusCode);
    }

    [Fact]
    public async Task LoginAsync_WhenUnexpectedError_ReturnsInternalServerError()
    {
        var request = new UserLoginRequest { Login = "login", Password = "password" };

        _authenticationMock
            .Setup(x => x.Authenticate(request.Login, request.Password))
            .ReturnsAsync(Result<(string, UserRefreshToken)>.Failure(CommonErrors.UnexpectedError));

        var controller = CreateController();

        var result = await controller.LoginAsync(request);

        var statusCode = Assert.IsType<StatusCodeResult>(result.Result);
        Assert.Equal(StatusCodes.Status500InternalServerError, statusCode.StatusCode);
    }

    [Fact]
    public async Task RefreshAsync_WhenSuccess_ReturnsOkWithTokens()
    {
        var refreshToken = CreateRefreshToken("new-refresh-token");
        var request = new RefreshTokenRequest
        {
            RefreshToken = new UserRefreshToken { Id = 7, Token = "old-refresh-token" },
        };

        _authenticationMock
            .Setup(x => x.RefreshAsync(request.RefreshToken))
            .ReturnsAsync(Result<(string, UserRefreshToken)>.Success(("new-access-token", refreshToken)));

        var controller = CreateController();

        var result = await controller.RefreshAsync(request);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<UserLoginResponse>(ok.Value);
        Assert.Equal("new-access-token", response.AccessToken);
        Assert.Same(refreshToken, response.RefreshToken);
    }

    [Fact]
    public async Task RefreshAsync_WhenFailed_ReturnsUnauthorizedProblem()
    {
        var request = new RefreshTokenRequest
        {
            RefreshToken = new UserRefreshToken { Id = 7, Token = "expired" },
        };

        _authenticationMock
            .Setup(x => x.RefreshAsync(request.RefreshToken))
            .ReturnsAsync(Result<(string, UserRefreshToken)>.Failure(AuthErrors.InvalidRefreshToken));

        var controller = CreateController();

        var result = await controller.RefreshAsync(request);

        var objectResult = Assert.IsAssignableFrom<ObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status401Unauthorized, objectResult.StatusCode);
    }

    [Fact]
    public async Task RefreshAsync_WhenFailed_ReturnsNeutralProblemTitle()
    {
        var request = new RefreshTokenRequest
        {
            RefreshToken = new UserRefreshToken { Id = 7, Token = "expired" },
        };

        _authenticationMock
            .Setup(x => x.RefreshAsync(request.RefreshToken))
            .ReturnsAsync(Result<(string, UserRefreshToken)>.Failure(AuthErrors.InvalidRefreshToken));

        var controller = CreateController();

        var result = await controller.RefreshAsync(request);

        var objectResult = Assert.IsAssignableFrom<ObjectResult>(result.Result);
        var problem = Assert.IsType<ProblemDetails>(objectResult.Value);
        Assert.Equal(AuthProblemDetails.RefreshTokenInvalid, problem.Title);
    }

    [Fact]
    public async Task IsUsernameAvailableAsync_WhenCalled_ReturnsOkWithResult()
    {
        var request = new IsUsernameAvailableRequest("username");

        _registrationMock
            .Setup(x => x.IsUsernameAvailableAsync(request.Username))
            .ReturnsAsync(true);

        var controller = CreateController();

        var result = await controller.IsUsernameAvailableAsync(request);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<IsUsernameAvailableResponse>(ok.Value);
        Assert.True(response.Result);
    }

    [Fact]
    public async Task IsLoginAvailableAsync_WhenCalled_ReturnsOkWithResult()
    {
        var request = new IsLoginAvailableRequest("login");

        _registrationMock
            .Setup(x => x.IsLoginAvailableAsync(request.Login))
            .ReturnsAsync(false);

        var controller = CreateController();

        var result = await controller.IsLoginAvailableAsync(request);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<IsLoginAvailableResponse>(ok.Value);
        Assert.False(response.Result);
    }
}